
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]

public class UdonCustomSample : UdonSharpBehaviour
{
    public UdonBehaviour soundudon;
    public MeshRenderer ColorChangeMesh;
    public TrailRenderer trail;
    public Material mat_off;
    public Material[] matlist;

    [UdonSynced,FieldChangeCallback(nameof(ColorID))]
    private int _ColorID = 0;
    [UdonSynced,FieldChangeCallback(nameof(light_on))]
    private bool _light_on=false;

    public int ColorID{
        set{
            _ColorID = value;
            ColorChangeMesh.material = matlist[ColorID];
            trail.material = matlist[ColorID];
        }
        get=>_ColorID;
    }
    

    public bool light_on{
        set{
            _light_on = value;
            if(value == true){
                trail.enabled = true;
                ColorChangeMesh.material = matlist[ColorID];
                trail.material = matlist[ColorID];
            }
            else
            {
                trail.enabled = false;
                ColorChangeMesh.material = mat_off;
            }
        }
        get => _light_on;
    }



    void Start()
    {
        
    }

    public void LightTurnOn(){
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        light_on = true;
        RequestSerialization();
        //Debug.Log("pickup");
    }

    public void LightTurnOFF(){
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        light_on = false;
        RequestSerialization();
        //Debug.Log("drop");
    }

    public void ChangeLightColor(){
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        if(ColorID >= matlist.Length - 1)ColorID=0;
        else ColorID++;
        RequestSerialization();
        soundudon.SendCustomEvent("SendSoundEvent");
        //Debug.Log("colorID"+ColorID.ToString());
    }




}
