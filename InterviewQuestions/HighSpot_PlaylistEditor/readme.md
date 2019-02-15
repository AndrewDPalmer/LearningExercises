#notes
- I time-boxed this to somewhere around 2 hours before documentation so I skipped some details I'd normally include.  They are noted below.

#How to use
- installe ruby (I kept it pretty vanilla with no extra dependencies)
- navigate to the folder in a terminal
- ruby playlist_processor.rb json/mixtape.json json/changes.json
- you can certainly change those paths to point at different files if you'd like (but again, the app currently doesn't do any file validation)
- output always goes to json/output.json
- the application posts a "-- #{success_message}" to stdout whenever it seccuessfully completes a change (or portion of)
- the application posts an "ERROR" to stdout whenver an item fails

#Sample App output to stdout
```
-- 0: ADDING SONG 1 TO PLAYLIST 1
-- 1: ERROR: SONG, DOES NOT EXIST
-- 2: ADDING PLAYLIST 4 TO USER 1
      ADDING SONG 1 TO PLAYLIST 4
      ADDING SONG 2 TO PLAYLIST 4
      ADDING SONG 3 TO PLAYLIST 4
      ERROR: SONG 99 DOES NOT EXIST FOR ADDITION TO PLAYLIST 4
-- 3: REMOVING PLAYLIST 1 FROM PLAYLIST DATASTORE
-- 4: ADDING Indestructible BY Robyn TO SONGS DATASTORE
-- 5: ADDING Dancing On My Own BY Robyn TO SONGS DATASTORE
-- 6: ADDING Time Machine BY Robyn TO SONGS DATASTORE
```

#Changes.json
- see json/changes.json for examples of each change supported
- each change has a name that I mapped to a method in the mixtape_service class, so you must use one of (add_song_to_playlist, create_playlist, remove_playlist, create_song)
- create_song wasn't required but I snuck it in so I could add a few favorites ;)

#assumptions
- all file paths input are assumed valid (I skipped input validation due to staying under 90 minute time-box)
- add_song (an unrequired change but one I snuck in) does not validate the existence of an existing song by title and artist so duplicates are currently possible
- test files don't include unit tests, they do however include the scenarios I used and would have been implemented if time wasn't a factor

#notable improvements that could have been made with another few hours
- create playlist, song, and user object classes, then used those instead of just doing json manipulation (much more manageable long term)
- error handling and input validation 
- working unit tests
- MixtapeService isn't written in the most testable manner and should be refactored a bit as well as DRYd up

#Improvements to handle massive json payloads
- parse the json files in chunks either manually, or using a library 
- while parsing the file in chunks, put each item into some sort of data store so we can tthen iterate on each item instead of keeping 100% of it in memory to work on.
- while we are at it, let's move our songs/playlists/users into some sort of datastore (likely an relational DB but could go with other options if scale justifies)
- those improvements would allow us to operate with only a few items in memory at a time.  Certain process would likely be faster if properly indexes (finding matching items in a table...)

