using System;
using UnityEngine;
using System.Collections;
using Cinch2D;
using Random = System.Random;

/// <summary>
/// Shows how to use mouse events.  Click the melon or text on the melon to make the bounce.
/// </summary>
public class TouchEvents : DemoBase {
    public override void OnAwake()
    {
        base.OnAwake();
        CreateBackground();
		
        //create a watermelon to listen to.  
        var watermelon = CinchSprite.NewFromImage("Cinch2D/Watermelon", 256);
        AddChild(watermelon);
        watermelon.Name = "Watermelon";
		
        //let's add a label to the melon
        var textField = SimpleTextField.NewFromString("Watermelon!", "Cinch2D/FjallaOne-Regular", .5f);
        watermelon.AddChild(textField);
        textField.Name = "TextField";
		
        //now add a listener.  Cinch2D implements MOUSE_DOWN, MOUSE_UP, RELEASE_OUTSIDE, MOUSE_OVER, MOUSE_OUT, and MOUSE_MOVE
        watermelon.AddEventListener<MouseEvent>(MouseEvent.TOUCH_DOWN, onWatermelonPress);
        watermelon.AddEventListener<MouseEvent>(MouseEvent.TOUCH_UP, onWatermelonRelease);
        watermelon.AddEventListener<MouseEvent>(MouseEvent.TOUCH_MOVE, onWatermelonDrag);
    }
	
    public void onWatermelonPress(MouseEvent e)
    {
        var currentTarget = (DisplayObjectContainer)e.CurrentTarget;
        new Tween(currentTarget, "ScaleX", 1.7f, .6f, Easing.Cubic.EaseIn).ContinueTo(1f, .2f, Easing.None.EaseNone);
    }
    
    public void onWatermelonRelease(MouseEvent e)
    {
        var currentTarget = (DisplayObjectContainer)e.CurrentTarget;
        new Tween(currentTarget, "ScaleY", 1.7f, .6f, Easing.Cubic.EaseIn).ContinueTo(1f, .2f, Easing.None.EaseNone);
    }
    
    public void onWatermelonDrag(MouseEvent e)
    {
        var currentTarget = (DisplayObjectContainer)e.CurrentTarget;
        currentTarget.RotationDegrees = new Random().Next(-20, 20);
    }
}
