
using Raylib_cs;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using static Raylib_cs.Raylib;

namespace SpellingNet
{
    unsafe
    internal class Program
    {
        private const int WIDTH = 1280;
        private const int HEIGHT = 720;
        private const int FONT_SIZE = 20;
        private const int FONT_SIZE_BIG = 20 * 2;
        private static Vector2 _mousePos = Vector2.Zero;
        private static string _uploadedFilePath = "Drag and drop a file to start.";
        private static string[] _uploadedFileLines = [];
        private static List<string> _currentList = [];
        private static Random _rand = new Random(Guid.NewGuid().GetHashCode());
        private static string _inputText = string.Empty;
        private const int MAX_SPEED = 10;
        private const int MIN_SPEED = -10;
        private static int _currentSpeed = -3;

        static void Main(string[] args)
        {
            CreateWindow();
            while (!WindowShouldClose())
            {
                _mousePos = GetMousePosition();
                BeginDrawing();
                ClearBackground(Color.Beige);
                DoWork();
                EndDrawing();
            }
        }

        private static void DoWork()
        {
            CompletionReportLabel();
            SpeedUpButton();
            SpeedDownButton();
            SpeedLabel();
            DragDrop();
            UploadedFileLabel();
            ResetButton();
            RandomizeButton();
            TextBox();
            SpeakButton();
            SubmitButton();
        }



        private static void CompletionReportLabel()
        {
            DrawText($"{_uploadedFileLines.Length - _currentList.Count} / {_uploadedFileLines.Length} Complete", 25, HEIGHT - 50, FONT_SIZE_BIG, Color.Black);
        }
        private static void SpeedUpButton()
        {
            if (ButtonClicked("FAST", (int)(WIDTH - 50), HEIGHT - 125))
            {
                _currentSpeed = _currentSpeed >= MAX_SPEED ? MAX_SPEED : _currentSpeed + 1;
            }
        }
        private static void SpeedDownButton()
        {
            if (ButtonClicked("SLOW", (int)(WIDTH - 50), HEIGHT - 50))
            {
                _currentSpeed = _currentSpeed <= MIN_SPEED ? MIN_SPEED : _currentSpeed - 1;
            }
        }
        private static void SpeedLabel()
        {
            DrawText(_currentSpeed.ToString(), WIDTH - 75, HEIGHT - 105, FONT_SIZE_BIG, Color.Black);
        }
        private static void SubmitButton()
        {
            if (ButtonClicked("Submit", (int)(WIDTH / 2), HEIGHT - 250) || IsKeyPressed(KeyboardKey.Enter))
            {
                if (_currentList.Any())
                {

                    if (_currentList.First().Equals(_inputText, StringComparison.OrdinalIgnoreCase))
                    {
                        // right
                        _currentList.RemoveAt(0);
                        Speak("Correct!");
                        if (!_currentList.Any())
                        {
                            Speak("Good Job! You Won!");
                        }
                        else
                        {
                            Speak(_currentList.First());
                        }
                    }
                    else
                    {
                        // wrong
                        _currentList.Add(_currentList.First());
                        _currentList.RemoveAt(0);
                        Speak("Incorrect");
                        Speak(_currentList.First());
                    }
                    _inputText = string.Empty;
                }
                else
                {
                    Speak("Drag and drop a file to start");
                }
            }
        }

        internal static void UpdateInputText()
        {
            if ((IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift)) && IsKeyDown(KeyboardKey.Backspace))
            {
                _inputText = string.Empty;
                return;
            }
            for (int i = (int)KeyboardKey.A; i <= (int)KeyboardKey.Z; i++)
            {
                if (IsKeyPressed((KeyboardKey)i))
                {
                    _inputText += (char)i;
                }
            }
            if (IsKeyPressed(KeyboardKey.Backspace))
            {
                if (_inputText.Length > 0)
                {
                    _inputText = _inputText.Substring(0, _inputText.Length - 1);
                }
            }
        }

        private static void TextBox()
        {
            UpdateInputText();
            Rectangle textBox = new Rectangle(50, HEIGHT / 3, WIDTH - 100, HEIGHT / 4);
            DrawRectangleRec(textBox, Color.RayWhite);
            DrawText(_inputText, (int)(textBox.X + 450), (int)(textBox.Y + 75), FONT_SIZE_BIG, Color.Black);
        }

        private static void UploadedFileLabel()
        {
            int textSize = MeasureText(_uploadedFilePath, FONT_SIZE);
            DrawText(_uploadedFilePath, WIDTH / 2 - textSize / 2, 50, FONT_SIZE, Color.RayWhite);
        }

        private static void DragDrop()
        {
            if (IsFileDropped())
            {
                FilePathList droppedFiles = LoadDroppedFiles();
                if (droppedFiles.Count == 1)
                {
                    _uploadedFilePath = Marshal.PtrToStringAnsi((IntPtr)(*droppedFiles.Paths));
                    UnloadDroppedFiles(droppedFiles);
                    _uploadedFileLines = File.ReadAllLines(_uploadedFilePath);
                    for (int i = 0; i < _uploadedFileLines.Length; i++)
                    {
                        string line = string.Empty;
                        for (int j = 0; j < _uploadedFileLines[i].Length; j++)
                        {
                            if (char.IsLetter(_uploadedFileLines[i][j]))
                            {
                                line += _uploadedFileLines[i][j];
                            }

                        }
                        _uploadedFileLines[i] = line.Trim().ToUpper();
                    }
                    _currentList = _uploadedFileLines.ToList();
                }
            }
        }
        private static void SpeakButton()
        {
            if (ButtonClicked("Speak", (int)(WIDTH / 2), HEIGHT / 2 - 175))
            {
                if (_currentList.Any())
                {
                    Speak(_currentList.First());
                }
                else
                {
                    Speak("There is nothing in the list to spell.");
                }
            }
        }


        private static void RandomizeButton()
        {
            if (ButtonClicked("Randomize List", (int)(WIDTH / 2 + 100), HEIGHT / 2 - 250))
            {
                RandomizeListInPlace(_currentList);
                if (!_currentList.Any())
                {
                    Speak("Drag and drop a file to start");
                }
                else
                {
                    Speak("List randomized");
                }
            }
        }

        private static void ResetButton()
        {
            if (ButtonClicked("Reset List", (int)(WIDTH / 2 - 100), HEIGHT / 2 - 250))
            {
                _currentList = _uploadedFileLines.ToList();
                if (!_currentList.Any())
                {
                    Speak("Drag and drop a file to start");
                }
                else
                {
                    Speak("List reset");
                }
            }
        }

        private static bool ButtonClicked(string text, int x, int y)
        {
            const int BUTTON_HEIGHT = 30;
            const int HORZ_PADDING = 3;
            const int VERT_PADDING = 5;
            int textLength = MeasureText(text, FONT_SIZE);
            Rectangle buttonRect = new Rectangle(x - textLength / 2, y - BUTTON_HEIGHT / 2, textLength + HORZ_PADDING * 2, BUTTON_HEIGHT);

            bool isMouseButtonPressed = IsMouseButtonPressed(MouseButton.Left);
            bool isOverButton = CheckCollisionPointRec(_mousePos, buttonRect);
            bool ret = false;
            if (isOverButton)
            {
                if (isMouseButtonPressed)
                {
                    DrawRectangleRec(buttonRect, Color.LightGray);
                    ret = true;
                }
                else
                {
                    DrawRectangleRec(buttonRect, Color.Gray);
                }
            }
            else
            {
                DrawRectangleRec(buttonRect, Color.DarkGray);
            }


            DrawText(text, (int)(buttonRect.X + 3), (int)(buttonRect.Y + VERT_PADDING), FONT_SIZE, Color.RayWhite);

            return ret;
        }

        private static void CreateWindow()
        {
            InitWindow(WIDTH, HEIGHT, "Spelling");

            SetWindowState(ConfigFlags.ResizableWindow);
            SetWindowMaxSize(WIDTH, HEIGHT);
            SetWindowMinSize(WIDTH, HEIGHT);
            // SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            // Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
            SetTargetFPS(30);
            InitAudioDevice();
        }

        static void Speak(string text)
        {
            Color shroud = new Color(0, 0, 0, 255 / 4);
            DrawRectangle(0, 0, WIDTH, HEIGHT, shroud);
            EndDrawing();
            using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
            {
                // Configure the synthesizer
                synthesizer.SelectVoiceByHints(VoiceGender.Female);

                synthesizer.Volume = 100;  // 0...100
                synthesizer.Rate = _currentSpeed;    // -10...10

                // Speak the text
                synthesizer.Speak(text);
            }
            BeginDrawing();
        }

        static void RandomizeListInPlace<T>(List<T> list)
        {
            int n = list.Count;

            for (int i = n - 1; i > 0; i--)
            {
                // Pick a random index from 0 to i
                int j = _rand.Next(0, i + 1);

                // Swap list[i] with the element at random index
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
