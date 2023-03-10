// ripped, presumably modifed from:
// https://github.com/chickensoft-games/go_dot_net

using System;
using System.Collections.Generic;

/// <summary>
/// Exception thrown when attempting to transition between states
/// that are incompatible.
/// </summary>
public class InvalidStateTransitionException<TState> : Exception
{
    /// <summary>Current state.</summary>
    public TState Current;
    /// <summary>Attempted next state which was invalid.</summary>
    public TState Desired;

    /// <summary>
    /// Creates a new invalid state transition exception.
    /// </summary>
    /// <param name="current">Current state.</param>
    /// <param name="desired">Attempted next state which was invalid.</param>
    /// <returns></returns>
    public InvalidStateTransitionException(TState current, TState desired) :
      base($"Invalid state transition between ${current} and ${desired}.")
    {
        Current = current;
        Desired = desired;
    }
}

/// <summary>
/// A record type that all machine states must inherit from.
///
/// Because records are reference types with value-based equality, states
/// can be compared easily by the state machine.
///
/// All state types must implement `CanTransitionTo` which returns true
/// if a given state can be transitioned to from the current state.
/// </summary>
public interface IMachineState<TState>
{
    /// <summary>
    /// Determines whether the given state can be transitioned to from the
    /// current state.
    /// </summary>
    /// <param name="state">The requested next state.</param>
    bool CanTransitionTo(TState state);
}

/// <summary>
/// Read-only interface for a machine. Expose machines as this interface
/// when you want to allow them to be observed and read, but not updated.
/// </summary>
public interface IReadOnlyStateMachine<TState> where TState : IMachineState<TState>
{
    /// <summary>
    /// Event handler for when the machine's state changes.
    /// </summary>
    /// <param name="state">The new state of the machine.</param>
    delegate void Changed(TState previousState, TState newState);

    /// <summary>Event emitted when the machine's state changes.</summary>
    event Changed? OnChanged;

    /// <summary>The current state of the machine.</summary>
    TState State { get; }
}

/// <summary>
/// A simple implementation of a state machine. Events an emit when the state
/// is changed.
///
/// Not intended to be subclassed ?????instead, use instances of this in a
/// compositional pattern.
///
/// States can implement `CanTransitionTo` to prevent transitions to invalid
/// states.
/// </summary>
/// <typeparam name="TState">Type of state used by the machine.</typeparam>
public sealed class StateMachine<TState> : IReadOnlyStateMachine<TState>
  where TState : IMachineState<TState>
{
    /// <summary>
    /// Creates a new machine with the given initial state.
    /// </summary>
    /// <param name="state">Initial state of the machine.</param>
    /// <param name="onChanged"></param>
    public StateMachine(
      TState state,
      IReadOnlyStateMachine<TState>.Changed? onChanged = null
    )
    {
        Announce(state);
        State = state;
        if (onChanged != null) { OnChanged += onChanged; }
    }

    /// <inheritdoc/>
    public TState State { get; private set; }

    /// <inheritdoc/>
    public event IReadOnlyStateMachine<TState>.Changed? OnChanged;

    /// <summary>
    /// Whether we're currently in the process of changing the state (or not).
    /// </summary>
    public bool IsBusy { get; private set; }

    private Queue<TState> _pendingTransitions { get; set; }
      = new Queue<TState>();

    /// <summary>
    /// Adds a value to the queue of pending transitions. If the next state
    /// is equivalent to the current state, the state will not be changed.
    /// If the next state cannot be transitioned to from the current state,
    /// the state will not be changed and a warning will be issued before
    /// attempting to transition to any subsequent queued states.
    /// </summary>
    /// <param name="value">State for the machine to transition to.</param>
    public void Update(TState value)
    {
        // Because machine state can be updated when firing state events from
        // previous state updates, we need to make sure we don't allow another
        // announce loop to begin while we're already announcing state updates.
        //
        // Instead, we just make sure we add the transition to the list of
        // pending transitions. State machines are guaranteed to enter each state
        // requested in the order they are requested (or throw an error if the
        // requested sequence is not comprised of valid transitions).
        _pendingTransitions.Enqueue(value);

        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        while (_pendingTransitions.Count > 0)
        {
            var state = _pendingTransitions.Dequeue();
            if (State.Equals(state))
            {
                continue;
            }

            if (State.CanTransitionTo(state))
            {
                Announce(state);
                State = state;
            }
            else
            {
                IsBusy = false;
                throw new InvalidStateTransitionException<TState>(State, state);
            }
        }

        IsBusy = false;
    }

    /// <summary>
    /// Announces the current state to any listeners.
    /// <see cref="Update"/> calls this automatically if the new state is
    /// different from the previous state.
    /// <br />
    /// Call this whenever you want to force a re-announcement.
    /// </summary>
    public void Announce(TState newState) => OnChanged?.Invoke(State, newState);
}
