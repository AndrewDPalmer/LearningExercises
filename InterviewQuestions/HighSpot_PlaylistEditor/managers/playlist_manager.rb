class PlaylistManager

  def initialize(playlists)
    @playlists = playlists
  end

  def to_hash
    return @playlists
  end

  def get(id)
    return_value = nil
    @playlists.each { |playlist| return_value = playlist if playlist["id"] == id  }
    return_value
  end

  def remove(id)
    @playlists.delete_if { |list| list["id"] == id }
  end

  def add(user_id)
    playlist_data = Hash.new
    playlist_data["id"] = (max_id + 1).to_s
    playlist_data["owner_id"] = user_id
    playlist_data["song_ids"] = Array.new
    @playlists.push(playlist_data)
    return playlist_data
  end

  private 
  def max_id
    return @playlists.max_by { |playlist| playlist["id"].to_i } ["id"].to_i
  end

end