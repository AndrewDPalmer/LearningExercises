class UserManager

    def initialize(users)
      @users = users
    end
  
    def to_hash
      return @users
    end

    def get(id)
      return_value = nil
      @users.each { |user| return_value = user if user["id"] == id  }
      return_value
    end
  
  end