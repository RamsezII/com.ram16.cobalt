﻿using _ARK_;
using _UTIL_;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _COBALT_
{
    public class Executor : IDisposable
    {
        public readonly ListListener<Executor> stack;
        public readonly Command command;

        static byte id_counter;
        public byte id = ++id_counter;

        public CommandLine line;
        public Command cmd_out = Command.cmd_echo;
        public readonly IEnumerator<CMD_STATUS> routine;
        public CMD_STATUS status;

        public readonly ThreadSafe_struct<bool> disposed = new();

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad() => id_counter = 0;

        //--------------------------------------------------------------------------------------------------------------

        public Executor(in ListListener<Executor> stack, in Command command)
        {
            this.stack = stack;
            this.command = command;

            if (command.routine != null)
                routine = command.routine(this);
        }

        //--------------------------------------------------------------------------------------------------------------

        public CMD_STATUS Iterate()
        {
            if (routine != null && routine.MoveNext())
                status = routine.Current;
            else
                Dispose();
            return status;
        }

        public CMD_STATUS Executate(in CommandLine line)
        {
            this.line = line;

            if (command._commands.Count == 0)
            {
                if (command.action != null)
                    command.action(line);
                else if (routine != null && routine.MoveNext())
                    status = routine.Current;
                else
                    Dispose();
            }
            else
            {
                status = new CMD_STATUS()
                {
                    state = CMD_STATE.WAIT_FOR_STDIN,
                    prefixe = $"{MachineSettings.machine_name.Value.SetColor("#73CC26")}:{NUCLEOR.terminal_path.SetColor("#73B2D9")}$",
                };

                if (line.ReadArgument(out string name_cmd1, out _, command._commands.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase)))
                    if (command._commands.TryGetValue(name_cmd1, out Command cmd1))
                    {
                        Executor executor = new(stack, cmd1);
                        if (line.signal == CMD_SIGNAL.EXEC)
                            stack.AddElement(executor);
                        executor.Executate(line);

                        if (line.TryReadPipe())
                            if (line.ReadArgument(out string name_cmd2, out _, Command.cmd_root_shell._commands.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase)))
                                Debug.Log($"\"{name_cmd1}\" piped into: \"{name_cmd2}\"");
                    }
                    else if (line.signal == CMD_SIGNAL.EXEC)
                        Debug.LogWarning($"Command not found: \"{name_cmd1}\"");
            }

            return status;
        }

        //--------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            lock (disposed)
            {
                if (disposed._value)
                    return;
                disposed._value = true;
                stack.RemoveElement(this);
                OnDispose();
            }
        }

        protected virtual void OnDispose()
        {
        }
    }
}