using Photon.Deterministic;


namespace Quantum
{
    public unsafe class CommandShareData : DeterministicCommand
    {
        public AssetRefInGameDataSettings ingameDataSettings;

        public override void Serialize(Photon.Deterministic.BitStream stream)
        {
            stream.Serialize(ref ingameDataSettings);
        }

        public void Execute(Frame frame)
        {
            //test용...? 내꺼랑 다른 사람 데이터가 맞는지 확인해보자...

            var otherData = frame.FindAsset<InGameDataSettings>(ingameDataSettings.Id);
            var myData = frame.FindAsset<InGameDataSettings>(frame.RuntimeConfig.InGameDataSettingsRef.Id);

            if (myData != null && otherData != null)
            {
                if (myData.playerDefaultData.AttackDuration != otherData.playerDefaultData.AttackDuration)
                {
                    Log.Error("Data Sync Error....!!!!!!!! AttackDuration >>> " + myData.playerDefaultData.AttackDuration + " | " + otherData.playerDefaultData.AttackDuration);
                }

                if (myData.playerDefaultData.AttackRange != otherData.playerDefaultData.AttackRange)
                {
                    Log.Error("Data Sync Error....!!!!!!!! AttackRange >>> " + myData.playerDefaultData.AttackRange + " | " + otherData.playerDefaultData.AttackRange);
                }

                if (myData.playerDefaultData.Input_AttackCooltime != otherData.playerDefaultData.Input_AttackCooltime)
                {
                    Log.Error("Data Sync Error....!!!!!!!! InputAttackCooltime >>> " + myData.playerDefaultData.Input_AttackCooltime + " | " + otherData.playerDefaultData.Input_AttackCooltime);
                }

                if (myData.ballDefaultData.BallIncreaseSpeed != otherData.ballDefaultData.BallIncreaseSpeed)
                {
                    Log.Error("Data Sync Error....!!!!!!!! BallIncreaseSpeed >>> " + myData.ballDefaultData.BallIncreaseSpeed + " | " + otherData.ballDefaultData.BallIncreaseSpeed);
                }

                if (myData.ballDefaultData.BallMaxSpeed != otherData.ballDefaultData.BallMaxSpeed)
                {
                    Log.Error("Data Sync Error....!!!!!!!! BallMaxSpeed >>> " + myData.ballDefaultData.BallMaxSpeed + " | " + otherData.ballDefaultData.BallMaxSpeed);
                }

                if (myData.ballDefaultData.BallMinSpeed != otherData.ballDefaultData.BallMinSpeed)
                {
                    Log.Error("Data Sync Error....!!!!!!!! BallMinSpeed >>> " + myData.ballDefaultData.BallMinSpeed + " | " + otherData.ballDefaultData.BallMinSpeed);
                }

                if (myData.ballDefaultData.BallRotationIncreaseSpeed != otherData.ballDefaultData.BallRotationIncreaseSpeed)
                {
                    Log.Error("Data Sync Error....!!!!!!!! BallRotationIncreaseSpeed >>> " + myData.ballDefaultData.BallRotationIncreaseSpeed + " | " + otherData.ballDefaultData.BallRotationIncreaseSpeed);
                }
            }
        }
    }
}
