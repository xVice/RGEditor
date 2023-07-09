using MelonLoader;
using RGEditor;
using MonkeyWebServer;
using System.Net;
using Il2CppNodeCanvas.Tasks.Actions;
using Il2Cpp;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Dynamic;
using JinianNet.JNTemplate;

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
        }

        public GadgetStruct GetCurrentGadget()
        {
            var currentGadget = FindObjectOfType<Gadget>();
            var cpus = currentGadget.cpus; //bad hell
            return new GadgetStruct(currentGadget.displayName, currentGadget.description);
        }

        public CPUStruct[] GetCurrentGadgetCPUS()
        {
            List<CPUStruct> cpusStructs = new List<CPUStruct>();
            var cpus = FindObjectOfType<SceneManager>().gadget.cpus.ToArray();
            Console.WriteLine("Cpus: "+ cpus.Length);
            foreach(var cpu in cpus)
            {
             
                var cpuStruct = new CPUStruct(cpu.GetCodeAsset().sourceCode);
                cpusStructs.Add(cpuStruct);
            }
            return cpusStructs.ToArray();
            
        }


        public struct CPUStruct
        {
            public string SourceCode { get; set; }

            public CPUStruct(string SourceCode)
            {
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

            
            return MonkeyResponse.RenderTemplate(req, "./Mods/templates/home.html", new { Gadget = gadget});
        }
    }
}
