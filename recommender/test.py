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