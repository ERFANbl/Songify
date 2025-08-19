import json
import math

def score(weekly_log: str, latest_log: str) -> list:
    weekly = json.loads(weekly_log)
    latest = json.loads(latest_log)
    songs_with_scores = []
    weekly_weights = [1, 0.75, 0.5, 0.25]
    alpha = 1.5  
    beta = 1
    
    for song_key, song_info in latest.items():
        song_id = song_key.split('#')[1]
        score = {"id": song_id}
        result = 0
        
        for i, week_data in enumerate(weekly.values()):
            song_weekly_info = week_data.get(song_key)
            
            if not song_weekly_info:
                continue
                
            try:
                duration_plays = song_info["song_duration"] * song_weekly_info["play_count"]
                if duration_plays == 0:
                    ratio = 0
                else:
                    ratio = song_weekly_info["time_listened"] / duration_plays
                    
                play_count = math.log10(song_weekly_info["play_count"] + 1)
                result += weekly_weights[i] * (play_count + beta * ratio)
                
                score["vectorId"] = song_weekly_info["vectorId"]
                
            except KeyError as e:
                print(f"Missing key {e} in song data")
                continue
                
        score["score"] = result + (alpha * song_info.get("is_liked", 0))
        songs_with_scores.append(score)
        
    return songs_with_scores 


weekly_example = '''
{
    "week1": {
        "song#123": {
            "time_listened": 180,
            "play_count": 3,
            "vectorId": "v1"
        }
    },
    "week2": {
        "song#123": {
            "time_listened": 300,
            "play_count": 5,
            "vectorId": "v2"
        }
    }
}
'''

latest_example = '''
{
    "song#123": {
        "song_duration": 60,
        "is_liked": 1
    }
}
'''

print(score(weekly_example, latest_example))

{
  "week1": {
    "song#1": { "time_listened": 210, "play_count": 2, "vectorId": "c638ddb5-28fd-4bb9-931a-56dc72c2c191" },
    "song#3": { "time_listened": 180, "play_count": 1, "vectorId": "9c22a851-6d3e-4999-96a4-bbfea1cceb95" },
    "song#7": { "time_listened": 320, "play_count": 3, "vectorId": "271b5a99-6b08-4e28-b444-0d0095ccb1a8" },
    "song#11": { "time_listened": 140, "play_count": 1, "vectorId": "9d834bc7-c1b5-44e5-8d64-bf6b40df48bd" }
  },
  "week2": {
    "song#2": { "time_listened": 250, "play_count": 2, "vectorId": "31d60bac-b0aa-420f-9bc3-a167364adfb9" },
    "song#5": { "time_listened": 300, "play_count": 3, "vectorId": "b0ec4bf5-15f9-403f-9853-c131aa11d47e" },
    "song#9": { "time_listened": 180, "play_count": 2, "vectorId": "ce248562-debc-48ed-8639-33b3f6f95435" },
    "song#12": { "time_listened": 270, "play_count": 2, "vectorId": "ad2aa037-83a9-441a-ba71-685e04b9f9af" }
  },
  "week3": {
    "song#4": { "time_listened": 200, "play_count": 2, "vectorId": "0057ebd5-93e4-4fed-a1c8-38c5da78a4f2" },
    "song#6": { "time_listened": 310, "play_count": 3, "vectorId": "e77cb058-4971-4fa7-b78a-bf0c27efff56" },
    "song#8": { "time_listened": 400, "play_count": 4, "vectorId": "c53bb4a2-0895-4c53-9931-5ecd61f730a1" },
    "song#13": { "time_listened": 250, "play_count": 2, "vectorId": "ce429f89-cfd9-4371-b842-d83bd6ca6571" }
  },
  "week4": {
    "song#1": { "time_listened": 160, "play_count": 1, "vectorId": "c638ddb5-28fd-4bb9-931a-56dc72c2c191" },
    "song#2": { "time_listened": 500, "play_count": 2, "vectorId": "31d60bac-b0aa-420f-9bc3-a167364adfb9" },
    "song#7": { "time_listened": 180, "play_count": 1, "vectorId": "271b5a99-6b08-4e28-b444-0d0095ccb1a8" },
    "song#10": { "time_listened": 260, "play_count": 1, "vectorId": "6b21cc6b-81bb-4bd1-aa32-9543b6119199" }
  }
}


{
  "song#1": { "song_duration": 233, "is_liked": 0 },
  "song#2": { "song_duration": 251, "is_liked": 1 },
  "song#3": { "song_duration": 286, "is_liked": 0 },
  "song#4": { "song_duration": 291, "is_liked": 0 },
  "song#5": { "song_duration": 309, "is_liked": 0 },
  "song#6": { "song_duration": 302, "is_liked": 0 },
  "song#7": { "song_duration": 320, "is_liked": 0 },
  "song#10": { "song_duration": 245, "is_liked": 0 },
  "song#12": { "song_duration": 301, "is_liked": 0 },
  "song#13": { "song_duration": 307, "is_liked": 0 }
}
