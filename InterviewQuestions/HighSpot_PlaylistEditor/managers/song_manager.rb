class SongManager

    def initialize(songs)
      @songs = songs
    end
  
    def to_hash
      return @songs
    end

    def get(id)
      return_value = nil
      @songs.each { |song| return_value = song if song["id"] == id  }
      return_value
    end

    def add(song_data)
      song_data["id"] = (max_id + 1).to_s
      @songs.push(song_data)
      return song_data
    end
  
    private 
    def max_id
      return @songs.max_by { |song| song["id"].to_i } ["id"].to_i
    end

  end