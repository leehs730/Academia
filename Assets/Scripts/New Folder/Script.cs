using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Script
{
    public Script(string __id, string __text, List<string> __next, string __result, string __sprite)
    {
        id = __id;
        text = __text;
        next = __next;
        result = __result;
        sprite = __sprite;
    }

    private string _id;
    private string _text;
    private List<string> _next;
    private string _result;
    private string _sprite;

    public string id
    {
        get { return _id; }
        set { _id = value; }
    }
    public string text
    {
        get { return _text; }
        set { _text = value; }
    }
    public List<string> next
    {
        get { return _next; }
        set { _next = value; }
    }

    private void Awake() 
    {
        _next = new List<string>();
    }
    public string result
    {
        get { return _result; }
        set { _result = value; }
    }
    public string sprite
    {
        get { return _sprite; }
        set { _sprite = value; }
    }
}
