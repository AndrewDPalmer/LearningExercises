require "json"
require_relative "../managers/playlist_manager"
require_relative "../managers/song_manager"
require_relative "../managers/user_manager"


class MixtapeService

  def initialize(source_file)
    input_data = File.read(source_file)
    json_data = JSON.parse(input_data)

    @users = UserManager.new(json_data["users"])
    @playlists = PlaylistManager.new(json_data["playlists"])
    @songs = SongManager.new(json_data["songs"])
  end

  def process_change_set(change_file_path)
    input_data = File.read(change_file_path)
    json_data = JSON.parse(input_data)
    @changes = json_data["changes"]

    @change_index = 0
    @changes.each do |change|
      process_change(change)
      @change_index = @change_index + 1
    end
  end

  def print_current_state(destination_path)
    output = File.open(destination_path, "w" )
    output << to_json
    output.close
  end

  def to_json
    JSON.pretty_generate(to_hash)
  end

  def to_hash
    hash = Hash.new
    hash["users"] = @users.to_hash
    hash["playlists"] = @playlists.to_hash
    hash["songs"] = @songs.to_hash
    return hash
  end

  private 
  def process_change(change)
    self.send(change["action"], change)
  end

  def add_song_to_playlist(change)
    song = @songs.get(change["song_id"])
    playlist = @playlists.get(change["playlist_id"])
    if (song && playlist)
      puts "-- #{@change_index}: ADDING SONG #{song["id"]} TO PLAYLIST #{playlist["id"]}"
      playlist["song_ids"].push(song["id"])
    else
      error_text = ''
      error_text += 'SONG, ' if !song
      error_text += 'PLAYLIST, ' if !playlist
      puts "-- #{@change_index}: ERROR: #{error_text}DOES NOT EXIST" 
    end
  end

  def create_playlist(change)
    user = @users.get(change["user_id"])
    if(!user)
      puts "   ERROR: USER DOES NOT EXIST"
      return
    end
    playlist = @playlists.add(user["id"])
    puts "-- #{@change_index}: ADDING PLAYLIST #{playlist["id"]} TO USER #{user["id"]}"
    change["song_ids"].each do |id|
      song = @songs.get(id)
      if song
        puts "      ADDING SONG #{song["id"]} TO PLAYLIST #{playlist["id"]}"
        playlist["song_ids"].push(song["id"])
      else
        puts "      ERROR: SONG #{id} DOES NOT EXIST FOR ADDITION TO PLAYLIST #{playlist["id"]}"
      end
    end
  end

  def remove_playlist(change)
    @playlists.remove(change["playlist_id"])
    puts "-- #{@change_index}: REMOVING PLAYLIST #{change["playlist_id"]} FROM PLAYLIST DATASTORE"
  end

  def create_song(change)
    @songs.add(change["song_data"])
    puts "-- #{@change_index}: ADDING #{change["song_data"]["title"]} BY #{change["song_data"]["artist"]} TO SONGS DATASTORE"
  end

end
