# PuttPutt
A discord bot for mini painting shame golf

# Commands
* `!fore [modifier]` Modifies your current score with the provided modifier. This modifier is required, so PuttPutt knows how much to adjust your score by
  *  Example: `!fore -2`
* `!scoreboard {limit}` Displays the scoreboard. Optionally, you can provide a limit value to only show the top N results
  *  Example: `!scoreboard`, `!scoreboard 10`
* `!myscore` Reports your current score
## Mod commands
* `!sync` Overwrites the database with values from usernames in the server. Currently, this pulls scores in a format of `[15]` or `{15}`, including negative scores. 

# Configuration
## Bot Authentication
You'll need a config.json file in the following format to connect locally:
```
{
  "bot_token": "YOUR_TOKEN_HERE"
}
```
If you're working on this for the shame golf league, message me here or on discord for the token

## Mongo
You'll need to either update the volume for mongo in the `docker-compose.yml` file to match where you want the database stored, or you can just remove the volumes entry from that service and let docker store it in the default location. 
