using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

////////////////////////////////////////////////////////////////////////
// GENERAL PURPOSE TASKS
////////////////////////////////////////////////////////////////////////

// Simple action task
public class ActionTask : Task {

    public Action _action { get; private set; }

    public ActionTask(Action action)
    {
        _action = action;
    }

    protected override void Init()
    {
        _action();
        SetStatus(TaskStatus.Success);
    }

}

public class OnGoingTask : Task
{
	private static readonly System.DateTime UnixEpoch = new System.DateTime(1970, 1, 1);
	private readonly double _duration; //how long does this wait for
	public Action _action { get; private set; }

	private double _startTime; //when did we start waiting

	private static double GetTimestamp()
    {
        return (System.DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
    }

	public OnGoingTask(Action action, double duration)
    {
        this._duration = duration;
		this._action = action;
    }

    protected override void Init()
    {
		_action();
        _startTime = GetTimestamp();
    }

    internal override void Update()
    {
        var now = GetTimestamp(); //use var for a) less typing, b) if it changes from float, to int, to double, etc.
        var durationElapsed = (now - _startTime) > _duration;

		_action();

        if (durationElapsed)
        {
            SetStatus(TaskStatus.Success);
        }
    }

}

public class WaitTask : Task
{
    // Get the timestamp in floating point milliseconds from the Unix epoch   
    private static readonly System.DateTime UnixEpoch = new System.DateTime(1970, 1, 1);

    private static double GetTimestamp()
    {
        return (System.DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
    }

    private readonly double _duration; //how long does this wait for
    private double _startTime; //when did we start waiting

    public WaitTask(double duration)
    {
        this._duration = duration;
    }

    protected override void Init()
    {
        _startTime = GetTimestamp();
    }

    internal override void Update()
    {
        var now = GetTimestamp(); //use var for a) less typing, b) if it changes from float, to int, to double, etc.
        var durationElapsed = (now - _startTime) > _duration;

        if (durationElapsed)
        {
            SetStatus(TaskStatus.Success);
        }
    }
}

// A base class for tasks that track time. Use it to make things like
// Wait, ScaleUpOverTime, etc. tasks
public abstract class TimedTask : Task
{
    public float Duration { get; private set; }
    public float StartTime { get; private set; }

    protected TimedTask(float duration)
    {
        Debug.Assert(duration > 0, "Cannot create a timed task with duration less than 0");
        Duration = duration;
    }

    protected override void Init()
    {
        StartTime = Time.time;
    }

    internal override void Update()
    {
        var now = Time.time;
        var elapsed = now - StartTime;
        var t = Mathf.Clamp01(elapsed / Duration);
        OnTick(t);
        if (t >= 1)
        {
            OnElapsed();
        }
    }

    // t is the normalized time for the task. E.g. if half the task's duration has elapsed then t == 0.5
    // This is where subclasses will do most of their work
    protected virtual void OnTick(float t) {}

    // Default to being successful if we get to the end of the duration
    protected virtual void OnElapsed()
    {
        SetStatus(TaskStatus.Success);
    }

}


// A VERY simple wait task
public class Wait : TimedTask
{
    public Wait(float duration) : base(duration) {}
}


////////////////////////////////////////////////////////////////////////
// GAME OBJECT TASKS
////////////////////////////////////////////////////////////////////////

// Base classes for tasks that operate on a game object.
// Since C# doesn't allow multiple inheritance we'll make two versions - one timed and one untimed
public abstract class GOTask : Task
{
    protected readonly GameObject gameObject;

    protected GOTask(GameObject gameObject)
    {
        this.gameObject = gameObject;
    }
}


public abstract class TimedGOTask : TimedTask
{
    protected readonly GameObject gameObject;

    protected TimedGOTask(GameObject gameObject, float duration) : base(duration)
    {
        this.gameObject = gameObject;
    }
}

// A task to teleport a gameobject
public class SetPos : GOTask
{
    private readonly Vector2 _pos;

    public SetPos(GameObject gameObject, Vector2 pos) : base(gameObject)
    {
        _pos = pos;
    }

    protected override void Init()
    {
        gameObject.transform.position = _pos;
        SetStatus(TaskStatus.Success);
    }
}


// A task to lerp a gameobject's position
public class LerpPos : TimedGOTask
{
    public Vector2 Start { get; private set; }
    public Vector2 End { get; private set; }

    public LerpPos(GameObject gameObject, Vector2 start, Vector2 end, float duration) : base(gameObject, duration)
    {
        Start = start;
        End = end;
    }

    protected override void OnTick(float t)
    {
        gameObject.transform.position = Vector2.Lerp(Start, End, t);
    }
}

public class LerpRotation : TimedGOTask
{
    private Quaternion Start;
    private Quaternion End;

    public LerpRotation(GameObject gameObject, Quaternion _start, Quaternion _end, float duration) : base(gameObject, duration)
    {
        Start = _start;
        End = _end;
    }

    protected override void OnTick(float t)
    {
        gameObject.transform.rotation = Quaternion.Lerp(Start, End, t);
    }
}

// A task to lerp a gameobject's scale
public class Scale : TimedGOTask
{
    public Vector2 Start { get; private set; }
    public Vector2 End { get; private set; }

    public Scale(GameObject gameObject, Vector2 start, Vector2 end, float duration) : base(gameObject, duration)
    {
        Start = start;
        End = end;
    }

    protected override void OnTick(float t)
    {
        gameObject.transform.localScale = Vector2.Lerp(Start, End, t);
    }
}

public class LerpObjectColor_Image : TimedTask
{
    private GameObject obj;
    private Color startColor;
    private Color endColor;
    private Easing.FunctionType easingType;

    public LerpObjectColor_Image(GameObject _obj, Color _start, Color _end, Easing.FunctionType _easingType, float duration) : base(duration)
    {
        obj = _obj;
        startColor = _start;
        endColor = _end;
        easingType = _easingType;
    }

    protected override void OnTick(float t)
    {
        Easing.Function easingFunction = Easing.GetFunctionWithTypeEnum(easingType);
        obj.GetComponent<Image>().color = Color.Lerp(startColor, endColor, easingFunction(t));
    }
}




