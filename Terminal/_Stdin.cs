﻿using _SGUI_;
using _UTIL_;
using System;
using UnityEngine;

namespace _COBALT_
{
    partial class Terminal
    {
        public readonly OnValue<KeyCode>
            flag_alt = new(),
            flag_nav_history = new();

        [SerializeField] string stdin_save;
        [SerializeField] int cpl_index;
        [SerializeField] int stdin_frame, tab_frame;

        //--------------------------------------------------------------------------------------------------------------

        void IMGUI_global.IUser.OnOnGUI()
        {
            Event e = Event.current;
            if (e.type != EventType.KeyDown)
                return;

            if (e.alt)
                switch (e.keyCode)
                {
                    case KeyCode.LeftArrow:
                    case KeyCode.RightArrow:
                    case KeyCode.UpArrow:
                    case KeyCode.DownArrow:
                        e.Use();
                        flag_alt.Update(e.keyCode);
                        break;
                }
            else if (!e.control && !e.command)
                switch (e.keyCode)
                {
                    case KeyCode.UpArrow:
                    case KeyCode.DownArrow:
                        e.Use();
                        flag_nav_history.Update(e.keyCode);
                        break;
                }
        }

        void OnAltKey()
        {
            CMD_SIGNALS signal = flag_alt.PullValue switch
            {
                KeyCode.LeftArrow => CMD_SIGNALS.ALT_LEFT,
                KeyCode.RightArrow => CMD_SIGNALS.ALT_RIGHT,
                KeyCode.UpArrow => CMD_SIGNALS.ALT_UP,
                KeyCode.DownArrow => CMD_SIGNALS.ALT_DOWN,
                _ => 0,
            };

            if (signal == 0)
                return;

            tab_frame = Time.frameCount;
            stdin_save = input_stdin.input_field.text;

            try
            {
                Command.Line line = new(
                    stdin_frame >= tab_frame ? input_stdin.input_field.text : stdin_save,
                    signal,
                    input_stdin.input_field.caretPosition
                    );

                executor.Executate(line);
                stdin_save = line.text;
                input_stdin.input_field.text = line.text;
                input_stdin.input_field.caretPosition = line.cursor_i;
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        void OnChangeStdin(string text)
        {
            if (tab_frame == Time.frameCount)
                return;

            cpl_index = 0;
            stdin_save = text;
            stdin_frame = Time.frameCount;
            flag_stdin.Update(true);
        }

        char OnValidateStdin(string text, int charIndex, char addedChar)
        {
            switch (addedChar)
            {
                case '\t':
                    tab_frame = Time.frameCount;
                    try
                    {
                        Command.Line line = new(
                            stdin_save,
                            CMD_SIGNALS.TAB,
                            Mathf.Min(stdin_save.Length, charIndex),
                            cpl_index++
                            );

                        executor.Executate(line);
                        input_stdin.input_field.text = line.text;
                        input_stdin.input_field.caretPosition = line.cursor_i;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e, this);
                    }
                    return '\0';

                case '\n':
                    cpl_index = 0;
                    stdin_save = null;
                    Debug.Log(input_prefixe.input_field.text + " " + input_stdin.input_field.text);
                    try
                    {
                        executor.Executate(new Command.Line(input_stdin.input_field.text, CMD_SIGNALS.EXEC));
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e, this);
                    }
                    input_stdin.input_field.text = null;
                    flag_stdin.Update(true);
                    return '\0';
            }
            return addedChar;
        }

        public void ClampBottom()
        {
            float bottom_view = -scrollview.viewport.rect.height - scrollview.content.anchoredPosition.y;
            float bottom_stdin = -input_stdout.text_height - input_stdin.text_height;

            if (bottom_stdin < bottom_view)
                scrollview.verticalNormalizedPosition = Mathf.InverseLerp(-scrollview.content.rect.height, -scrollview.viewport.rect.height, bottom_stdin - 2 * line_height);
        }
    }
}