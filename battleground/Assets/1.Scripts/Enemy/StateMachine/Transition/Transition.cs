using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Transition
{
    public Decision decision; // Transition을 하기위한 decision
    public State trueState; // true에 해당할때 이동하는 State
    public State falseState; // false에 해당할때 이동하는 State
}
