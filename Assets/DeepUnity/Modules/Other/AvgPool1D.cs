using System;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


namespace DeepUnity.Modules
{
    /// <summary>
    /// Input: <b>(B, C, H_in)</b> or <b>(C, H_in)</b> <br></br>
    /// Output: <b>(B, C, H_out)</b> or <b>(C, H_out)</b> <br></br>
    /// where B = batch_size, C = channels, H_in = input_size and <br></br>H_out = Floor((H_in + 2 * padding - kernel_size - 1) / kernel_size + 1)<br></br>
    /// </summary>
    [SerializeField]
    public class AvgPool1D : IModule
    {
        private Tensor InputCache { get; set; }

        [SerializeField] private int kernelSize;
        [SerializeField] private int padding;
        [SerializeField] private PaddingType paddingMode;


        /// <summary>
        /// Input: <b>(B, C, H_in)</b> or <b>(C, H_in)</b> <br></br>
        /// Output: <b>(B, C, H_out)</b> or <b>(C, H_out)</b> <br></br>
        /// where B = batch_size, C = channels, H_in = input_size and <br></br>H_out = Floor((H_in + 2 * padding - kernel_size - 1) / kernel_size + 1)<br></br>
        /// </summary>
        public AvgPool1D(int kernel_size, int padding = 0, PaddingType padding_mode = PaddingType.Zeros)
        {
            if (padding < 0)
                throw new ArgumentException("Padding cannot be less than 0");
            if (kernel_size < 2)
                throw new ArgumentException("Kernel Size cannot be less than 2");

            this.kernelSize = kernel_size;
            this.padding = padding;
            this.paddingMode = padding_mode;
        }

        public Tensor Predict(Tensor input)
        {
            if (input.Rank != 2 && input.Rank != 3)
                throw new ShapeException($"Input({input.Shape.ToCommaSeparatedString()}) must either be (B, C, H) or (C, H).");

            if (padding > 0)
                Tensor.VecPad(input, padding, paddingMode);

            bool isBatched = input.Rank == 3;
            int batch_size = isBatched ? input.Size(-3) : 1;
            int channel_size = input.Size(-2);
            int H_in = input.Size(-1);
            int H_out = (int)Math.Floor((H_in + 2 * padding - 1 * (kernelSize - 1) - 1) / (float)kernelSize + 1);

            if (H_out < 1)
                throw new ShapeException($"The input shape {input.Shape.ToCommaSeparatedString()} is smaller than the kernel {kernelSize} in avg1d pooling layer.");


            Tensor output = isBatched ?
                Tensor.Zeros(batch_size, channel_size, H_out) :
                Tensor.Zeros(channel_size, H_out);

            Parallel.For(0, batch_size, b =>
            {
                Parallel.For(0, channel_size, c =>
                {
                    LinkedList<float> values_pool = new LinkedList<float>();

                    for (int j = 0; j < H_out; j++)
                    {

                        for (int ki = 0; ki < kernelSize; ki++)
                        {
                            try
                            {
                                values_pool.AddLast(input[b, c, j * kernelSize + ki]);
                            }
                            catch { }
                        }


                        output[b, c, j] = values_pool.Average();
                        values_pool.Clear();
                    }
                });
            });

            return output;
        }

        public Tensor Forward(Tensor input)
        {
            InputCache = Tensor.Identity(input);

            return Predict(input);
        }

        public Tensor Backward(Tensor loss)
        {
            bool isBatched = loss.Rank == 3;
            int Batch = isBatched ? loss.Size(-3) : 1;
            int Channels = loss.Rank >= 2 ? loss.Size(-2) : 1;
            int H_out = loss.Size(-1);
            int H_in = InputCache.Size(-1);

            Tensor gradInput = isBatched ?
                Tensor.Zeros(Batch, Channels, H_in) :
                Tensor.Zeros(Channels, H_in);


            Parallel.For(0, Batch, b =>
            {
                Parallel.For(0, Channels, c =>
                {
                    for (int j = 0; j < H_out; j++)
                    {
                        float averageValue = loss[b, c, j] / kernelSize;

                        for (int pi = 0; pi < kernelSize; pi++)
                        {
                            int rowIndex = j * kernelSize + pi;

                            if (rowIndex >= 0 && rowIndex < H_in)
                            {
                                gradInput[b, c, rowIndex] += averageValue;
                            }
                        }
                    }
                });
            });



            return gradInput;
        }



        public object Clone() => new AvgPool1D(kernelSize, padding, paddingMode);

    }


}

