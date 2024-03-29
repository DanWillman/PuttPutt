# PuttPutt
A discord bot for mini painting shame golf

[![Build And Test](https://github.com/DanWillman/PuttPutt/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/DanWillman/PuttPutt/actions/workflows/build-and-test.yml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=DanWillman_PuttPutt&metric=coverage)](https://sonarcloud.io/dashboard?id=DanWillman_PuttPutt)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=DanWillman_PuttPutt&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=DanWillman_PuttPutt)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=DanWillman_PuttPutt&metric=alert_status)](https://sonarcloud.io/dashboard?id=DanWillman_PuttPutt)

* `!fore [modifier] {Reason}` Modifies your current score with the provided modifier. This modifier is required, so PuttPutt knows how much to adjust your score by. Optionally, any text after the modifier will be stored as a note for this event in history. 
  *  Example: `!fore -2`, `!fore -2 Painted some new duder`
* `!setscore [score] {Reason}` Sets your current score to the provided score. Optionally, any text after the score will be stored as a note for this event in history.
  *  Example: `!setscore 0`, `!setscore 0 Resetting for some reason`
* `!scoreboard {limit}` Displays the scoreboard. Optionally, you can provide a limit value to only show the top N results
  *  Example: `!scoreboard`, `!scoreboard 10`
* `!history {limit}` Displays your history. Optionally, you can provide a limit value to only show the top N results
  *  Example: `!history`, `!history 10`
* `!archives` Gets a list of all archive names, allowing the user to then pull up a scoreboard for that season
* `!seasonscores [archive]` Displays the scoreboard for the specified archive (season) name. Hint: use `!archives` to get the list, or check out [shame.golf](shame.golf)
  *  Example: `!seasonscores Summer2021`
* `!seasonhistory [limit]|{0} [archive]` Displays the user history for a given season, and limits the results if the limit constraint is not 0. Hint: use `!archives` to get the list, or check out [shame.golf](shame.golf)
  *  Example: `!seasonscores 0 Summer2021`, `!seasonscores 5 Summer2021`
* `!myscore` Reports your current score
## Mod commands
* `!sync` Overwrites the database with values from usernames in the server. Currently, this pulls scores in a format of `[15]` or `{15}`, including negative scores. 
* `!season {name}` Archives the current season and begins anew. If you provide a name, it will be used, otherwise a timestamp will be used.
  *  Example: `!season`, `!season Summer2021`
* `!updatenames` Updates all user names on scoreboard to their sanitized (no score, remove excess spaces) names.

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
