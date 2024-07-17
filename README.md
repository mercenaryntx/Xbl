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
![image](https://github.com/user-attachments/assets/3a47e8b7-9830-4458-8321-3157c0b364e9)


# Rarest achievements
TODO

```
Xbl -r
```
![image](https://github.com/user-attachments/assets/357cc33e-bc28-4200-a160-1cdd6de9f7e2)

# Most complete games
TODO

```
Xbl -m
```
![image](https://github.com/user-attachments/assets/8948a72c-e6d8-4459-8645-b722acdb61ae)


# Most played games
TODO

```
Xbl -s
```
![image](https://github.com/user-attachments/assets/92a87181-e68a-4547-8d4b-f72abea1716a)


# Xbox 360 Profile support
TODO

```
Xbl -p={Xbox360ProfilePath} [-c|m]
```
![image](https://github.com/user-attachments/assets/2aee471d-bcaf-47a0-90d3-0b7c7325ebf1)
![image](https://github.com/user-attachments/assets/64bb7518-d8a7-40c7-9143-79625121e55f)


# JSON output
TODO

```
Xbl {YourCommand} -o=json
```

# Output limit
TODO

```
Xbl {YourCommand} -l={MaxNumberOfItems}
```
