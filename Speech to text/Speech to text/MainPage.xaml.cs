using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using Microsoft.CognitiveServices.Speech;
using Speech_to_text.Services;

namespace Speech_to_text
{
    public partial class MainPage : ContentPage
    {
        SpeechRecognizer recognizer;
        IMicrophoneService micService;
        bool isTranscribing = false;

        public MainPage()
        {
            InitializeComponent();

            micService = DependencyService.Resolve<IMicrophoneService>();
        }


        async void TranscribeClicked(object sender, EventArgs e)
        {
            bool isMicEnabled = await micService.GetPermissionAsync();

            // EARLY OUT: make sure mic is accessible
            if (!isMicEnabled)
            {
                UpdateTranscription("Please grant access to the microphone!");
                return;
            }

            // initialize speech recognizer 
            if (recognizer == null)
            {
                var config = SpeechTranslationConfig.FromSubscription(Constants.CognitiveServicesApiKey, Constants.CognitiveServicesRegion);

                config.SpeechRecognitionLanguage = "pt-BR";
 
                recognizer = new SpeechRecognizer(config);
                recognizer.Recognized += (obj, args) =>
                {
                    UpdateTranscription(args.Result.Text);
                };
            }

            // if already transcribing, stop speech recognizer
            if (isTranscribing)
            {
                try
                {
                    await recognizer.StopContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
                    UpdateTranscription(ex.Message);
                }
                isTranscribing = false;
            }

            // if not transcribing, start speech recognizer
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    InsertDateTimeRecord();
                });
                try
                {
                    await recognizer.StartContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
                    UpdateTranscription(ex.Message);
                }
                isTranscribing = true;
            }
            UpdateDisplayState();
        }


        void UpdateTranscription(string newText)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    transcribedText.Text += $"{newText}\n";
                }
            });
        }

        void InsertDateTimeRecord()
        {
            var msg = $"======================\nAGLET 2023\n{DateTime.Now.ToString()}\n======================";
            UpdateTranscription(msg);
        }

        void UpdateDisplayState()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (isTranscribing)
                {
                    transcribeButton.Text = "Parar";
                    transcribeButton.BackgroundColor = Color.Red;
                    transcribingIndicator.IsRunning = true;
                }
                else
                {
                    transcribeButton.Text = "Falar";
                    transcribeButton.BackgroundColor = Color.Green;
                    transcribingIndicator.IsRunning = false;
                }
            });
        }

        private void clearButton_Clicked(object sender, EventArgs e)
        {
            transcribedText.Text = "";
        }
    }
}
