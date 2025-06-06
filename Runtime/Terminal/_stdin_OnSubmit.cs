﻿using _COBRA_;
using UnityEngine;

namespace _COBALT_
{
    partial class Terminal
    {
        void OnSubmit()
        {
            cpl_index = 0;
            stdin_save = string.Empty;
            string input_text = input_stdin.input_field.text;
            input_stdin.ResetText();

            if (shell.current_state.status.state == CMD_STATES.WAIT_FOR_STDIN)
            {
                if (string.IsNullOrWhiteSpace(input_text))
                {
                    ToggleWindow(false);
                    return;
                }

                string lint_text = linter.GetLint(shell, input_text, out _);
                Debug.Log(input_prefixe.input_field.text + " " + lint_text, this);
            }

            Command.Line line = new(input_text, SIG_FLAGS.CHECK, shell);
            string error = shell.PropagateSignal(line);

            if (error == null)
            {
                line = new(input_text, SIG_FLAGS.SUBMIT, shell);
                bool was_idle = shell.IsIdle;

                error = shell.PropagateSignal(line);

                if (was_idle && error == null)
                    Command.Line.AddToHistory(line.text);
            }
        }
    }
}