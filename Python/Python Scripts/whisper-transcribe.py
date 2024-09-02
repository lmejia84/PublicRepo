'''
Script used to transcribe audio to text by utilizing OpenAI's Whisper open-source model: https://github.com/openai/whisper. My gaming computer has an NVIDIA RTX-3080 GPU, 
which allows me to run the Whisper model locally without using the OpenAI API (which can be expensive). I wrote a script a while ago that records a live stream called "La Hora del Té."
The script I worked on now cuts segments of the audio where there are commercials and then creates a new file. After the file is created, it utilizes the Whisper model to transcribe 
the audio to text. The purpose of this is to create a GPT with knowledge from about 10 years' worth of transcribed live streams. 
Unfortunately, I didn't document all the steps I took to make this work; I just pulled an all-nighter until it worked.

Here are the steps I remember following:
- Install PyTorch with CUDA support for NVIDIA GPUs: pip install torch torchvision torchaudio --extra-index-url https://download.pytorch.org/whl/cu118
- This works with NVIDIA GPUs. You can also use your computer's CPU to transcribe, but it takes longer, and a different version of PyTorch needs to be installed.
- Install Whisper: pip install git+https://github.com/openai/whisper.git
- Download and install Chocolatey: https://chocolatey.org/
- Download and install FFmpeg: https://www.ffmpeg.org/download.html
- Download the NVIDIA CUDA Toolkit: https://developer.nvidia.com/cuda-toolkit
- Add FFmpeg to the system path. If this doesn't work, you may need to copy the files to a directory like C:\ffmpeg\bin and then point the path within the script to this location.
'''
from pydub import AudioSegment
import os
import whisper
import datetime

#function to add start and end times for the transcription, I currently have this turned off for the purpose of what I am working on, but is a nice feature to have.
def convert_to_milliseconds(time_str):
    """Converts time in hh:mm:ss format to milliseconds."""
    hours, minutes, seconds = map(int, time_str.split(':'))
    return (hours * 3600 + minutes * 60 + seconds) * 1000


def trim_mp3(input_file, output_file, segments_to_remove):
    """
    Trims the specified segments from the input MP3 file and saves the result to the output file.
    :param input_file: Path to the input MP3 file.
    :param output_file: Path to the output MP3 file.
    :param segments_to_remove: List of tuples specifying the start and end times as (start_time, end_time) in hh:mm:ss format.
    """

    audio = AudioSegment.from_mp3(input_file)
    
    # Convert the HH:MM:SS segments to milliseconds to remove them from the file.
    segments_in_ms = [(convert_to_milliseconds(start_time), convert_to_milliseconds(end_time))
                      for start_time, end_time in segments_to_remove]
    
    # Sort the segments to remove in reverse order to avoid messing up the indices (Used chatGPT for this.)
    segments_in_ms.sort(key=lambda x: x[0])
    offset = 0
    for start, end in segments_in_ms:
        start -= offset
        end -= offset
        audio = audio[:start] + audio[end:]
        offset += (end - start)
    
    # Export the trimmed audio to a new MP3 file
    audio.export(output_file, format="mp3")


# Example usage:
input_mp3 = r"C:\Users\lmeji\OneDrive\LHT\LHT\LHT-20240814175501.mp3" #Original file with commercials
output_mp3 = r"C:\\Users\\lmeji\\OneDrive\\Github\Whisper GPU\\audio\\"+os.path.basename(input_mp3) #New file without commercials

# Define the segments to remove (start and end times in hh:mm:ss). Basically, there are commercials during these minutes.
segments_to_remove = [
    ("00:00:00", "00:08:32 "),
    ("01:23:38", "01:38:11"),
    ("02:00:37","02:20:10")
]

trim_mp3(input_mp3, output_mp3, segments_to_remove)







'''
Start using the whiste model to transcribe the audio file.
'''
print("finalizó cortando los segmentos")
print("Empieza a transcribir")

# Set the environment variable to point to the ffmpeg installation, adding this to PATH did not work for some reason.
os.environ["PATH"] += os.pathsep + r"C:\ffmpeg\bin"

# Load the Whisper model (choose a model for better accuracy, e.g., "medium" or "large")
model = whisper.load_model("medium") #base, tiny, medium, large. (Medium transcribes very well and is a lot faster than Large. 1.5 hours are transcribed within 20 minutes with Medium and 6 hours with Large.)

# Transcribe the audio file with specified language and timestamps, the file that is input here is the output file from the previous step where we cut the segments.
input_mp3_to_transcribe = output_mp3
result = model.transcribe(input_mp3_to_transcribe, language="es", verbose=False)  # Set verbose=True if you want to see the transcription in the Terminal.

# Function to convert seconds to HH:MM:SS format
def format_timestamp(seconds):
    return str(datetime.timedelta(seconds=int(seconds)))

# Save the transcription to a text file with HH:MM:SS timestamps
transcribed_text = r"C:\\Users\\lmeji\\OneDrive\\Github\Whisper GPU\transcribed\\"+os.path.basename(input_mp3_to_transcribe.replace(".mp3",".txt"))
with open(transcribed_text, "w", encoding="utf-8") as f:
    for segment in result["segments"]:
        # Format the timestamp as HH:MM:SS
        # start_time = format_timestamp(segment["start"])   #Uncomment this and the times will be added to the transcript.
        # end_time = format_timestamp(segment["end"])       #Uncomment this and the times will be added to the transcript.
        text = segment["text"]
        # f.write(f"[{start_time} - {end_time}] {text}\n")
        f.write(f"{text}\n")

print(f"Transcription saved to {transcribed_text}.")