from fastapi import FastAPI, HTTPException,  Form
from SongRecommender.SongRecommenderService import RecommendSong
import json

def extract_unique_song_vectors(json_string):

    data = json.loads(json_string)

    songs = []
    seen_songIds = []
    
    for week_data in data.values():
        for song_key, song_info in week_data.items():
            if '#' in song_key:
                song_id = song_key.split('#')[1]
                if song_id not in seen_songIds:
                    songs.append({"id": song_id, "vectorId": song_info.get('vectorId')})
                    seen_songIds.append(song_id)

    return songs

app = FastAPI()

@app.post("/MadeForUser")
async def MadeForUser(
    WeeklySongsLogs: str = Form(...),
    UserVectorId: str = Form(...),
):
    try:
        uniqeSongVectorIds = extract_unique_song_vectors(WeeklySongsLogs)
        recommendedsongs = RecommendSong(UserVectorId, uniqeSongVectorIds, num_recommend=5)
        return str(recommendedsongs[:20])[2:-2]

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8002)

