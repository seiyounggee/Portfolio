using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Deterministic;

namespace Quantum.Custom
{
    public unsafe class GamePlaySystem : SystemMainThread, 
        ISignalOnComponentRemoved<GameManager>,
        ISignalOnComponentRemoved<PlayerRules>,
        ISignalOnPlayerDataSet
    {
        public override void OnInit(Frame f)
        {
            base.OnInit(f);

            if (f == null)
                return;

            //게임 매니저 만들어주고 Init 때려주자...!
            var gameManager = f.Unsafe.GetOrAddSingletonPointer<GameManager>();
            if (gameManager != null)
            {
                gameManager->OnInit(f);
            }
        }

        public void OnRemoved(Frame f, EntityRef entity, GameManager* component)
        {
            component->OnRemoved(f, entity, component);
        }

        public void OnRemoved(Frame f, EntityRef entity, PlayerRules* component)
        {
            component->Onremoved(f, entity, component);
        }

        public override void Update(Frame f)
        {
            if (f == null)
                return;

            //게임 매니저 Update 때려주자...!
            if (f.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
            {
                gameManager->Update(f);
            }
        }

        public void OnPlayerDataSet(Frame frame, PlayerRef player)
        {
            SetGamePlaySettings(frame); //요 타이밍때 꼭 해주자...
            SetCustomMapDataSettings(frame); //요 타이밍때 꼭 해주자...

            if (frame.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
            {
                gameManager->SpawnPlayer(frame, player);
                gameManager->SpawnAIPlayer(frame, player); //여러번 호출해도 1번만 소환시킴
            }
        }

        private void SetGamePlaySettings(Frame frame)
        {
            var settings = frame.FindAsset<GamePlaySettings>(frame.RuntimeConfig.GamePlaySettingsRef.Id);
            if (settings == null)
                return;

            settings.Real_PlayerNumber = frame.PlayerCount;
            settings.AI_PlayerNumber = settings.Total_PlayerNumber - frame.PlayerCount;

            if (frame.Unsafe.TryGetPointerSingleton<GameManager>(out var gameManager))
            {
                gameManager->SetInGamePlayMode(frame, settings.InGamePlayMode);
                gameManager->SetData(frame);
            }
        }

        private void SetCustomMapDataSettings(Frame frame)
        {
            var mapSettings = frame.FindAsset<CustomMapDataSettings>(frame.RuntimeConfig.CustomMapDataSettingsRef.Id);
            if (mapSettings == null)
                return;

            mapSettings.SuffleSpawnPoint(frame);
        }
    }
}