# Xbl
A simple Xbox achievement stats tool

# Prerequisites
Xbl is built on OpenXBL, an unofficial Xbox Live API.
In order to use Xbl first you are going to need an OpenXBL registration and a personal API key.

1. Go to [xbl.io](http://xbl.io)
2. Login with your Xbox account
3. On the profile page create a new API key
4. Save it, as you won't be able to recover it from the site later

> [!WARNING]
> The **free** tier of OpenXBL gives you **150 requests/day** only. Please note that the "requests/hour" title is not correct on the OpenXBL profile page, moreover it takes about 25 hours to reset the counter. 

# Data update
Because of the daily limitations of OpenXBL you probably won't be able to get all your data at once, but do not worry Xbl can work with partial data as well and can incrementally update your data later. By default Xbl downloads both achievements and stats, but in order to save requests you can limit update to either `achievements` or `stats`.

```
Xbl -a={YourApiKey} -u
Xbl -a={YourApiKey} -u=all
Xbl -a={YourApiKey} -u=achievements
Xbl -a={YourApiKey} -u=stats
```

> [!NOTE]
> If you limit your update to either `achievements` or `stats` you can update 149 titles at one try, so if you have about 500 games you'll need four days to download everything. If you need everything that's about 7 days.

# Quick summary
TODO

```
Xbl -c
```

# Rarest achievements
TODO

```
Xbl -r
```

# Most complete games
TODO

```
Xbl -m
```

# Most played games
TODO

```
Xbl -s
```

# Xbox 360 Profile support
TODO

```
Xbl -p={Xbox360ProfilePath} -c
```

# JSON output
TODO

# Output limit
TODO
