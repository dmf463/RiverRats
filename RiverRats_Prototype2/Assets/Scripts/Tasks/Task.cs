using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task
{

    public enum TaskStatus : byte
    {
        Detached, // Task has not been attached to a TaskManager     
        Pending, // Task has not been initialized     
        Working, // Task has been initialized     
        Success, // Task completed successfully     
        Fail, // Task completed unsuccessfully     
        Aborted // Task was aborted 
    }

    // The only member variable that a base task has is its status public TaskStatus Status { get; private set; }  
    public TaskStatus Status { get; private set; }

    // Convenience status checking 
    public bool IsDetached { get { return Status == TaskStatus.Detached; } }
    public bool IsAttached { get { return Status != TaskStatus.Detached; } }
    public bool IsPending { get { return Status == TaskStatus.Pending; } }
    public bool IsWorking { get { return Status == TaskStatus.Working; } }
    public bool IsSuccessful { get { return Status == TaskStatus.Success; } }
    public bool IsFailed { get { return Status == TaskStatus.Fail; } }
    public bool IsAborted { get { return Status == TaskStatus.Aborted; } }
    public bool IsFinished { get { return (Status == TaskStatus.Fail || Status == TaskStatus.Success || Status == TaskStatus.Aborted); } }

    // Convenience method for external classes to abort the task 
    public void Abort()
    {
        SetStatus(TaskStatus.Aborted);
    }

    // A method for changing the status of the task 
    internal void SetStatus(TaskStatus newStatus)
    {
        if (Status == newStatus) return;
        Status = newStatus;
        switch (newStatus)
        {
            case TaskStatus.Working:
                //initialize task when the task first starts
                //important to separate initialization from the constructor,
                //since tasks may not start running until long after they've been constructed
                Init();
                break;
            //success/aborted/failed are the completed states of the task
            //subclasses are notified when entering one of these states
            //and are given the oppurtunity to do any clean up
            case TaskStatus.Success:
                OnSuccess();
                CleanUp();
                break;
            case TaskStatus.Aborted:
                OnAbort();
                CleanUp();
                break;
            case TaskStatus.Fail:
                OnFail();
                CleanUp();
                break;
            // These are "internal" states that are relevant for         
            // the task manager         
            case TaskStatus.Detached:
            case TaskStatus.Pending:
                break;
            default:
                throw new System.ArgumentOutOfRangeException(newStatus.ToString(), newStatus, null);
        }
    }
    //Delegation methods
    //you have some base class that calls virtual methods and calls subclasses to do the work
    //the subclasses are responsible for DOING the work. This is a workflow thing
    //base class DELEGATES the work to subclasses. I AM THE CAPTAIN NOW
    protected virtual void OnAbort() { }

    protected virtual void OnSuccess() { }

    protected virtual void OnFail() { }

    // Override this to handle initialization of the task. 
    // This is called when the task enters the Working state 
    protected virtual void Init() { }

    // Called whenever the TaskManager updates. Your tasks' work 
    // generally goes here 
    internal virtual void Update() { }

    // This is called when the tasks completes (i.e. is aborted, 
    // fails, or succeeds). It is called after the status change 
    // handlers are called 
    protected virtual void CleanUp() { }

    // Assign a task to be run if this task runs successfully 
    public Task NextTask { get; private set; }

    // Sets a task to be automatically attached when this one completes successfully 
    // NOTE: if a task is aborted or fails, its next task will not be queued 
    // NOTE: **DO NOT** assign attached tasks with this method. 
    public Task Then(Task task)
    {
        Debug.Assert(!task.IsAttached);
        NextTask = task;
        return task;
    }
}
