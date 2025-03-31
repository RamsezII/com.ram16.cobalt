﻿using _ARK_;
using _UTIL_;
using UnityEngine;

namespace _COBALT_
{
    partial class Terminal
    {
        //prefixe = $"{MachineSettings.machine_name.Value.SetColor("#73CC26")}:{NUCLEOR.terminal_path.SetColor("#73B2D9")}$";
        public readonly ListListener<Command.Executor> executors_stack = new();
        public static readonly Command shell = new(
            false,
            default,
            line => new CMD_STATUS()
            {
                state = CMD_STATE.WAIT_FOR_STDIN,
                prefixe = $"{MachineSettings.machine_name.Value.SetColor("#73CC26")}:{NUCLEOR.terminal_path.SetColor("#73B2D9")}$"
            },
            null);

        //--------------------------------------------------------------------------------------------------------------

        static void InitShell()
        {
            shell.commands.Clear();
        }

        //--------------------------------------------------------------------------------------------------------------

        void AwakeShell()
        {
            Command.Executor executor = new(executors_stack, shell);
            executors_stack.AddElement(executor);

            shell.commands.Add("help", new Command(
                false,
                default,
                line =>
                {
                    foreach (var pair in shell.commands)
                        Debug.Log($"{pair.Key} : \"{pair.Value.manual}\"");
                    return new CMD_STATUS();
                },
                null
            ));

            executors_stack._list[^1].Executate(new CommandLine(string.Empty, CMD_SIGNAL._NONE_));
        }
    }
}