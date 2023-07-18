using MelonLoader;
using RGEditor;
using MonkeyWebServer;
using System.Net;
using Il2CppNodeCanvas.Tasks.Actions;
using Il2Cpp;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Dynamic;
using Il2CppUI.Apps;

[assembly: MelonInfo(typeof(EntryPoint), "RGEdit", "0.0.1", "Jupe")]
[assembly: MelonGame("Evil Licorice", "Retro Gadgets")]

namespace RGEditor
{
    public class EntryPoint : MelonMod
    {
        
        SDK sdk;

        public override void OnInitializeMelon()
        {
            ClassInjector.RegisterTypeInIl2Cpp<SDK>();
            GameObject retroSDKGameObject = new GameObject("ModSDKHolder");
            retroSDKGameObject.AddComponent<SDK>();
            sdk = UnityEngine.Object.FindObjectOfType<SDK>();
        }

        public override void OnApplicationLateStart()
        {
            MonkeyEndpoints.AddEndpoint(new HomePage(sdk));
            MonkeyEndpoints.AddEndpoint(new SetCpuCode(sdk));


            MonkeyServer.StartServer(true, true);
            MonkeyServer.AddPrefix("http://localhost:8000/");
            Task.Run(MonkeyServer.HandleIncomingConnections);
        }

        public override void OnApplicationQuit()
        {
            MonkeyServer.StopServer();
        }

    }

    public class SDK : MonoBehaviour
    {
        public void Awake()
        {
            MelonLogger.Msg("Sdk loaded");
            //MultiTool.instance.LogMessageToConsole(MultiTool.LogType.Info, "RetroEdit sdk loaded!");
        }

        public GadgetStruct GetCurrentGadget()
        {
            var currentGadget = FindObjectOfType<Gadget>();
            return new GadgetStruct(currentGadget.displayName, currentGadget.description);
        }

        public SceneManager GetSceneManager()
        {
            return FindObjectOfType<SceneManager>();
        }

        public void SetCpuCode(int index, string code)
        {
            FindObjectOfType<DebugApp>().SetCodeAsset(new CodeAsset($"CPU{index}.lua", code)); //<-- :D

            /*

            var gadget = GetSceneManager().gadget;
            var assetContainer = gadget.assetContaniner;
            foreach(var assetdict in assetContainer.assets)
            {
                foreach(var asset in assetdict.Value.values)
                {
                    if(asset.GetAssetType() == AssetType.Code)
                    {
                        asset.
                    }
                }

             
            }
            */


        }

    

 

        public CPUStruct[] GetCurrentGadgetCPUS()
        {
            List<CPUStruct> cpusStructs = new List<CPUStruct>();
            var cpus = FindObjectOfType<SceneManager>().gadget.cpus.ToArray();
            foreach(var cpu in cpus)
            {
                var cpuStruct = new CPUStruct(cpu.GetCodeAsset().name, cpu.GetCodeAsset().sourceCode);
                
                cpusStructs.Add(cpuStruct);
            }
            return cpusStructs.ToArray();
            
        }


        public struct CPUStruct
        {
            public string Name { get; set; }
            public string SourceCode { get; set; }

            public CPUStruct(string Name, string SourceCode)
            {
                this.Name = Name;
                this.SourceCode = SourceCode;
            }
        }

        public struct GadgetStruct
        {
            public string DisplayName { get; set; }
            public string Description { get; set; }
            
            public GadgetStruct(string displayName, string Description)
            {
                this.DisplayName = displayName;
                this.Description = Description;
  
            }
        }
    }

    public class SetCpuCode : MonkeyEndpoint
    {
        public override string Endpoint { get => "/setcpucode/"; }

        SDK sdk;

        public SetCpuCode(SDK sdk)
        {
            this.sdk = sdk;
        }

        public override MonkeyResponse Execute(MonkeyRequest req)
        {
            var request = req.jsonReq;

            sdk.SetCpuCode(request.Value<int>("CPUIndex"), request.Value<string>("Code"));
            var cpu = sdk.GetCurrentGadgetCPUS()[request.Value<int>("CPUIndex")];

            return MonkeyResponse.Json(req, cpu);
        }
    }

    public class HomePage : MonkeyEndpoint
    {
        public override string Endpoint { get => "/editor"; }

        SDK sdk;

        public HomePage(SDK sdk)
        {
            this.sdk = sdk;
        }

        public override MonkeyResponse Execute(MonkeyRequest req)
        {
            var gadget = sdk.GetCurrentGadget();
            var cpus = sdk.GetCurrentGadgetCPUS();

            return MonkeyResponse.RenderTemplate(req, "./Mods/templates/home.html", new { Gadget = gadget, CPUS = cpus});
        }
    }
}
