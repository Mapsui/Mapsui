﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Animations;

public static class Animation
{
    // Sync object for enries list
    private static readonly object _syncObject = new();

    /// <summary>
    /// List of all active animations
    /// </summary>

    /// <summary>
    /// Start a single AnimationEntry
    /// </summary>
    /// <param name="entry">AnimationEntry to start</param>
    /// <param name="duration">Duration im ms for the given AnimationEntry</param>
    public static void Start<T>(AnimationEntry<T> entry, long duration)
    {
        Start(entry, duration, DateTime.Now.Ticks);
    }

    /// <summary>
    /// Start a list of AnimationEntrys
    /// </summary>
    /// <remarks>All AnimationEntries are started at the same time.</remarks>
    /// <param name="entries">List of AnimationEntry to start</param>
    /// <param name="duration">Duration im ms for the given AnimationEntry</param>
    public static void Start<T>(IEnumerable<AnimationEntry<T>> entries, long duration)
    {
        // Start all animations in entries with the same ticks
        var ticks = DateTime.Now.Ticks;

        foreach (var entry in entries)
            Start(entry, duration, ticks);
    }

    /// <summary>
    /// Start of a AnimationEntry with given duration at given ticks for StartTicks
    /// </summary>
    /// <remarks>When the ticks is given, more than one AnimationEntry with the same StartTicks could be started.</remarks>
    /// <param name="entry">AnimationEntry to start</param>
    /// <param name="duration">Duration im ms for the given AnimationEntry</param>
    /// <param name="ticks">StartTicks for this AnimationEntry</param>
    private static void Start<T>(AnimationEntry<T> entry, long duration, long ticks)
    {
        lock (_syncObject)
        {
            entry.StartTicks = ticks;
            entry.DurationTicks = duration * TimeSpan.TicksPerMillisecond;
            entry.EndTicks = entry.StartTicks + entry.DurationTicks;
        }
    }

    /// <summary>
    /// Stop a given AnimationEntry
    /// </summary>
    /// <param name="entry">AnimationEntry to stop</param>
    /// <param name="callFinal">Final function is called, if callFinal is true</param>
    public static AnimationResult<T> Stop<T>(T target, AnimationEntry<T> entry, bool callFinal = true)
    {
        entry.Done = true;

        if (entry == null)
            return new AnimationResult<T>(target, false);

        if (callFinal)
        {
            return entry.Final(target);
        }
        return new AnimationResult<T>(target, false);
    }

    /// <summary>
    /// Stop all AnimationEntries in a given list
    /// </summary>
    /// <param name="entries">AnimationEntry to stop</param>
    /// <param name="callFinal">Final function is called, if callFinal is true</param>
    public static void Stop<T>(T target, IEnumerable<AnimationEntry<T>> entries, bool callFinal = true)
    {
        foreach (var entry in entries)
            Stop(target, entry, callFinal);
    }

    /// <summary>
    /// Update all AnimationEntrys and check, if a redraw is needed
    /// </summary>
    /// <returns>True, if a redraw of the screen is needed</returns>
    public static AnimationResult<T> UpdateAnimations<T>(T target, IEnumerable<AnimationEntry<T>> entries)
    {
        var ticks = DateTime.Now.Ticks;

        AnimationEntry<T>[] entriesArray;

        entriesArray = entries.ToArray();

        if (entriesArray.Length == 0)
            return new AnimationResult<T>(target, false);

        var isRunning = false;

        for (var i = 0; i < entriesArray.Length; i++)
        {
            if (ticks > entriesArray[i].EndTicks)
            {
                // Animation is at the end of duration
                if (!entriesArray[i].Repeat)
                {
                    // Animation shouldn't be repeated, so remove it
                    target = Stop(target, entriesArray[i], true).State;
                    continue;
                }

                // Set new values for repeating this animation
                entriesArray[i].StartTicks = entriesArray[i].EndTicks;
                entriesArray[i].EndTicks = entriesArray[i].StartTicks + entriesArray[i].DurationTicks;
            }

            var value = (ticks - entriesArray[i].StartTicks) / (double)entriesArray[i].DurationTicks;

            if (value < entriesArray[i].AnimationStart || value > entriesArray[i].AnimationEnd)
            {
                // Nothing to do before the animation starts or after animation ended
                continue;
            }

            var result = entriesArray[i].Tick(target, value);
            isRunning |= result.IsRunning;
            target = result.State;
        }

        return new AnimationResult<T>(target, isRunning);
    }
}
