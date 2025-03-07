using DeepUnity.ReinforcementLearning;
using UnityEngine;

namespace DeepUnity.Tutorials
{
    public class ShooterScript : Agent
    {
        // Spring: 3000 | Damper: 100 | MaxForce: 6000

        [Header("Needs normalization")]
        public GlockScript Glock;
        public GameObject head;
        public GameObject torso;
        public GameObject chest;

        public GameObject armL;
        public GameObject forearmL;
        public GameObject handL;

        public GameObject armR;
        public GameObject forearmR;
        public GameObject handR;

        public GameObject legL;
        public GameObject shinL;
        public GameObject footL;

        public GameObject legR;
        public GameObject shinR;
        public GameObject footR;

        BodyController bodyController;
        public override void Awake()
        {
            base.Awake();

            bodyController = GetComponent<BodyController>();

            // 16 body parts
            bodyController.AddBodyPart(head);
            bodyController.AddBodyPart(chest);
            bodyController.AddBodyPart(torso);

            bodyController.AddBodyPart(armL);
            bodyController.AddBodyPart(forearmL);
            bodyController.AddBodyPart(handL);

            bodyController.AddBodyPart(armR);
            bodyController.AddBodyPart(forearmR);
            bodyController.AddBodyPart(handR);

            bodyController.AddBodyPart(legL);
            bodyController.AddBodyPart(shinL);
            bodyController.AddBodyPart(footL);

            bodyController.AddBodyPart(legR);
            bodyController.AddBodyPart(shinR);
            bodyController.AddBodyPart(footR);

            bodyController.bodyPartsList.ForEach(bodypart =>
            {
                if (bodypart.gameObject != footL.gameObject && bodypart.gameObject != footR.gameObject && bodypart.gameObject != shinL.gameObject && bodypart.gameObject != shinR.gameObject)
                {
                    bodypart.ColliderContact.OnEnter = (col) =>
                    {
                        if (col.collider.CompareTag("Ground"))
                        {
                            AddReward(-1f);
                            EndEpisode();
                        }
                    };
                }
            });
        }

        // 128 input variables + 3 from ray
        public override void CollectObservations(StateVector stateBuffer)
        {
            var jdDict = bodyController.bodyPartsDict;

            // + 8
            stateBuffer.AddObservation(Glock.StateOneHot);
            stateBuffer.AddObservation(Glock.Ammo);
            stateBuffer.AddObservation(Glock.transform.rotation);


            // + 4
            stateBuffer.AddObservation(chest.transform.rotation);

            // + 10
            BodyPart _head = jdDict[head];
            {
                stateBuffer.AddObservation(_head.rigidbody.velocity);
                stateBuffer.AddObservation(_head.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_head.CurrentNormalizedEulerRotation);
                stateBuffer.AddObservation(_head.CurrentNormalizedStrength);
            }

            // + 6
            BodyPart _chest = jdDict[chest];
            {
                stateBuffer.AddObservation(_chest.rigidbody.velocity);
                stateBuffer.AddObservation(_chest.rigidbody.angularVelocity);
            }

            // + 10
            BodyPart _torso = jdDict[torso];
            {
                stateBuffer.AddObservation(_torso.rigidbody.velocity);
                stateBuffer.AddObservation(_torso.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_torso.CurrentNormalizedEulerRotation);
                stateBuffer.AddObservation(_torso.CurrentNormalizedStrength);
            }

            // + 36
            BodyPart _armR = jdDict[armR];
            BodyPart _armL = jdDict[armL];
            BodyPart _faR = jdDict[forearmR];
            BodyPart _faL = jdDict[forearmL];
            {
                stateBuffer.AddObservation(_armR.rigidbody.velocity);
                stateBuffer.AddObservation(_armR.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_armR.CurrentNormalizedEulerRotation);
                stateBuffer.AddObservation(_armR.CurrentNormalizedStrength);

                stateBuffer.AddObservation(_armL.rigidbody.velocity);
                stateBuffer.AddObservation(_armL.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_armL.CurrentNormalizedEulerRotation);
                stateBuffer.AddObservation(_armL.CurrentNormalizedStrength);



                stateBuffer.AddObservation(_faR.rigidbody.velocity);
                stateBuffer.AddObservation(_faR.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_faR.CurrentNormalizedEulerRotation.x);
                stateBuffer.AddObservation(_faR.CurrentNormalizedStrength);

                stateBuffer.AddObservation(_faL.rigidbody.velocity);
                stateBuffer.AddObservation(_faL.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_faL.CurrentNormalizedEulerRotation.x);
                stateBuffer.AddObservation(_faL.CurrentNormalizedStrength);
            }

            // + 54
            BodyPart _legR = jdDict[legR];
            BodyPart _legL = jdDict[legL];
            BodyPart _shinR = jdDict[shinR];
            BodyPart _shinL = jdDict[shinL];
            BodyPart _footR = jdDict[footR];
            BodyPart _footL = jdDict[footL];
            {
                stateBuffer.AddObservation(_legR.rigidbody.velocity);
                stateBuffer.AddObservation(_legR.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_legR.CurrentNormalizedEulerRotation);
                stateBuffer.AddObservation(_legR.CurrentNormalizedStrength);

                stateBuffer.AddObservation(_legL.rigidbody.velocity);
                stateBuffer.AddObservation(_legL.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_legL.CurrentNormalizedEulerRotation);
                stateBuffer.AddObservation(_legL.CurrentNormalizedStrength);

                stateBuffer.AddObservation(_shinR.rigidbody.velocity);
                stateBuffer.AddObservation(_shinR.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_shinR.CurrentNormalizedEulerRotation.x);
                stateBuffer.AddObservation(_shinR.CurrentNormalizedStrength);

                stateBuffer.AddObservation(_shinL.rigidbody.velocity);
                stateBuffer.AddObservation(_shinL.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_shinL.CurrentNormalizedEulerRotation.x);
                stateBuffer.AddObservation(_shinL.CurrentNormalizedStrength);

                stateBuffer.AddObservation(_footR.rigidbody.velocity);
                stateBuffer.AddObservation(_footR.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_footR.CurrentNormalizedEulerRotation.x);
                stateBuffer.AddObservation(_footR.CurrentNormalizedStrength);
                stateBuffer.AddObservation(_footR.ColliderContact.IsGrounded);

                stateBuffer.AddObservation(_footL.rigidbody.velocity);
                stateBuffer.AddObservation(_footL.rigidbody.angularVelocity);
                stateBuffer.AddObservation(_footL.CurrentNormalizedEulerRotation.x);
                stateBuffer.AddObservation(_footL.CurrentNormalizedStrength);
                stateBuffer.AddObservation(_footL.ColliderContact.IsGrounded);
            }
        }

        // 36 continuous actions, 3 discrete actions
        public override void OnActionReceived(ActionBuffer actionBuffer)
        {
            var jdDict = bodyController.bodyPartsDict;

            float[] actions_vector = actionBuffer.ContinuousActions;

            int i = 0;
            jdDict[head].SetJointTargetRotation(actions_vector[i++], actions_vector[i++], actions_vector[i++]);
            jdDict[torso].SetJointTargetRotation(actions_vector[i++], actions_vector[i++], actions_vector[i++]);

            jdDict[armL].SetJointTargetRotation(actions_vector[i++], actions_vector[i++], actions_vector[i++]);
            jdDict[forearmL].SetJointTargetRotation(actions_vector[i++], 0f, 0f);

            jdDict[armR].SetJointTargetRotation(actions_vector[i++], actions_vector[i++], actions_vector[i++]);
            jdDict[forearmR].SetJointTargetRotation(actions_vector[i++], 0f, 0f);

            jdDict[legL].SetJointTargetRotation(actions_vector[i++], actions_vector[i++], actions_vector[i++]);
            jdDict[shinL].SetJointTargetRotation(actions_vector[i++], 0f, 0f);
            jdDict[footL].SetJointTargetRotation(actions_vector[i++], 0f, 0f);

            jdDict[legR].SetJointTargetRotation(actions_vector[i++], actions_vector[i++], actions_vector[i++]);
            jdDict[shinR].SetJointTargetRotation(actions_vector[i++], 0f, 0f);
            jdDict[footR].SetJointTargetRotation(actions_vector[i++], 0f, 0f);


            jdDict[head].SetJointStrength(actions_vector[i++]);
            jdDict[torso].SetJointStrength(actions_vector[i++]);

            jdDict[armL].SetJointStrength(actions_vector[i++]);
            jdDict[forearmL].SetJointStrength(actions_vector[i++]);

            jdDict[armR].SetJointStrength(actions_vector[i++]);
            jdDict[forearmR].SetJointStrength(actions_vector[i++]);

            jdDict[legL].SetJointStrength(actions_vector[i++]);
            jdDict[shinL].SetJointStrength(actions_vector[i++]);
            jdDict[footL].SetJointStrength(actions_vector[i++]);

            jdDict[legR].SetJointStrength(actions_vector[i++]);
            jdDict[shinR].SetJointStrength(actions_vector[i++]);
            jdDict[footR].SetJointStrength(actions_vector[i++]);


            Collider hit = null;
            switch(actionBuffer.DiscreteAction)
            {
                case 0:
                    break;
                case 1:
                    Glock.Fire(out hit);
                    break;
                case 2:
                    Glock.Reload();
                    break;

            }

            AddReward(0.001f);
            if(hit != null && hit.CompareTag("Target"))
            {
                AddReward(+0.5f);
            }
        }
    }


}

