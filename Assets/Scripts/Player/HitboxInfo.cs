using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxInfo : MonoBehaviour
{
    private MoveProperty properties;
    public MoveProperty Properties {
        get {return properties;}
    }

    private ActionController owner;
    public ActionController Owner {
        get {return owner;}
    }

    public void Init(MoveProperty property, ActionController owner) {
        SetProperties(property);
        SetOwner(owner);
    }

    public void SetProperties(MoveProperty property) {
        properties = property;
    }
    public void SetOwner(ActionController owner) {
        this.owner = owner;
    }
}
