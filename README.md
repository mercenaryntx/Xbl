# Xbl CLI (Working title)
**Xbl CLI** is a powerful command line tool designed to provide detailed statistics on your Xbox achievements.<br>
Whether you’re using **Xbox Live** or have an old **Xbox 360** profile, this tool has you covered.

# Features
- **Comprehensive Support**: Works seamlessly with both Xbox Live and Xbox 360 profiles.
- **Built-in Queries**: Utilize pre-defined queries to quickly access the data you need.
- **KQL Capabilities**: Leverage Kusto Query Language (KQL) for advanced data analysis and custom queries.

# Prerequisites
## Xbox Live
**Xbl CLI** is built on OpenXBL, an unofficial Xbox Live API.
In order to use **Xbl CLI** first you are going to need an OpenXBL registration and a personal API key.

1. Go to [xbl.io](http://xbl.io)
2. Login with your Xbox account
3. On the profile page create a new API key
4. Save it, as you won't be able to recover it from the site later

> [!WARNING]
> The **free** tier of OpenXBL gives you **150 requests/day** only. Please note that the "requests/hour" title is not correct on the OpenXBL profile page, moreover it takes about 25 hours to reset the counter. 

Because of the daily limitations of OpenXBL you probably won't be able to get all your data at once, but do not worry **Xbl CLI** can work with partial data as well and can incrementally update your data later. By default **Xbl CLI** downloads both achievements and stats, but in order to save requests you can limit update to either `achievements` or `stats`.

```
Xbl -a={YourApiKey} -u
Xbl -a={YourApiKey} -u=all
Xbl -a={YourApiKey} -u=achievements
Xbl -a={YourApiKey} -u=stats
```

> [!NOTE]
> If you limit your update to `achievements` you can update 149 titles at one try, while `stats` works in pages (100 games per page), so if you have about 500 games you'll need four days to download everything anyway.

## Xbox 360
**Xbl CLI** can also import old Xbox 360 profiles (also known as STFS packages).

To get your Xbox 360 profile file, follow the steps of the [official Xbox Support article](https://support.xbox.com/en-US/help/xbox-360/my-account/manage-gamertag-and-profile/move-your-profile-to-another-console-using-a-flash-drive-or-memory-unit).<br>
Or if you have an RGH console can use my old [GODspeed](https://github.com/mercenaryntx/godspeed) tool.

Once you have your profile file, you can use the following command to import your data:

```
Xbl -p={YourProfilePath}
```

# Built-in queries
## Quick summary about your imported profiles
This command gives you a small aggregation of data: 
- number of games
- number of unlocked achievements
- number of Gamerscore
- the recorded hours of gameplay (Xbox Live only)

In the footer you'll get a sum of all games / unique titles.
```
Xbl -q=summary
```
![image](https://github.com/user-attachments/assets/8ba3d28c-4acc-4f90-91d9-f5105e8490cc)

## Your most complete games
This command orders your games based on progress percentage.
```
Xbl -q=completeness
```
![image](https://github.com/user-attachments/assets/5c3f7484-19c6-498b-9641-24a6ff830785)

# Games you spent the most time with
This command generates a detailed breakdown chart showcasing the games you’ve logged the most hours on.<br>
Please note that not all games support this feature, so you might not see data for certain titles, like The Witcher 3. :)
```
Xbl -q=time
```
![image](https://github.com/user-attachments/assets/07e81849-d90f-4da0-8258-4642ddbd661d)

## Your rarest achievements
This command orders your achievements based on their rarity.
```
Xbl -q=rarity
```
![image](https://github.com/user-attachments/assets/44c29141-ec60-4dde-9893-bf24ca0c0a84)

## Your games with the most rarest achievements
This command uses an exponential scale to order your games with the most valuable achievements.<br>
The **R/A/T** column stands for **R**are/**A**chieved/**T**otal.

$`
weight = \frac{G}{\sum G} \cdot e^{\frac{\text{rarity}}{5}}
`$
<br><br>

```
Xbl -q=weighted-rarity
```
![image](https://github.com/user-attachments/assets/30deaa1a-4f2a-434d-8381-5c1a214c0a9e)

## Item limit
The default item limit of the queries above is 50. To change that use the `-l` argument.

```
Xbl {YourCommand} -l={MaxNumberOfItems}
```

## JSON output
All commands (except the quick summary) supports JSON output so you can import the generated data in other tools like Excel or Flourish Studio (see below).

```
Xbl {YourCommand} -o=json
```

> [!TIP]
> <img src="https://github.com/user-attachments/assets/b3d8b271-94cb-4f0e-ad78-fd63abf050ee" alt="Weighted Rarity bubble chart generated with Flourish" width="500"/><br>
> This bubble chart was created with [Flourish](https://app.flourish.studio/) based on the following query: `Xbl -q=weighted-rarity -l=1000 -o=json`

# Kusto (KQL) queries
If you would like to play with custom queries and harness the capabilities of KQL, use the `-k` argument.<br>
Both `.kql` files and inline queries are supported.

```
Xbl -k=MyKustomQuery.kql
Xbl -k="achievements | summarize count() by Platform"
```
## Data Tables
In your KQL query you can use three different data tables: `titles`, `achievements` and `stats`. Stats are only available for Xbox Live data.

### titles
```json
{                                                             
   "TitleId": "1915865634",                                   
   "Name": "Lorem Ipsum: The Game",                           
   "CompatibleWith": "PC|Mobile|Xbox360|XboxOne|XboxSeries",  
   "CurrentAchievements": 18,                                 
   "TotalAchievements": 0,                                    
   "CurrentGamerscore": 560,                                  
   "TotalGamerscore": 1000,                                   
   "ProgressPercentage": 56,                                  
   "LastTimePlayed": "2025-02-02T16:54:38.4848326+01:00",     
   "Category": "Shooter",                                     
   "Xbox360ProductId": "66acd000-77fe-1000-9115-d802305807d3",
   "XboxOneProductId": "9MVHHT21X5F6",                        
   "XboxSeriesProductId": "bx3zk30jl2dl",                     
   "Xbox360ReleaseDate": "2020-01-01T01:00:00+01:00",         
   "XboxOneReleaseDate": "2020-01-01T01:00:00+01:00",         
   "XboxSeriesReleaseDate": "2020-01-01T01:00:00+01:00"       
}
```
### achievements
```json
{                                                  
   "Name": "Lorem ipsum",                          
   "TitleId": "10027721",                          
   "TitleName": "dolor sit amet",                  
   "IsUnlocked": true,                             
   "TimeUnlocked": "2016-09-11T14:25:10.686+02:00",
   "Platform": "Xbox360|XboxOne|XboxSeries",       
   "IsSecret": false,                              
   "Description": "Lorem ipsum",                   
   "LockedDescription": "dolor sit amet",          
   "Gamerscore": 100,                              
   "IsRare": false,                                
   "RarityPercentage": 12.34                       
}
```
### stats
```json
{                                                  
    "XUID": "2533274845176708",                    
    "SCID": "40950100-bb3f-4769-906a-5eae3b67330a",
    "TitleId": "996619018",                        
    "Minutes": 341                                 
}
```

> [!TIP]
> To get the schema of the given tables you can use the `-e` (Extended help) argument as well.
> ```
> Xbl -e
> ```

## Rendering charts
**Xbl CLI** uses [Kusto-Loco](https://github.com/NeilMacMullen/kusto-loco) to support KQL queries that can also render the results as a chart. Charts are rendered to HTML files using the [Vega-Lite](https://vega.github.io/vega-lite/examples/) charting library.

### Example
<img src="https://github.com/user-attachments/assets/4cbb5d56-d01f-4319-9fc2-78b33d1619d3" alt="Custom KQL chart example" width="700"/>
<br><br>

This Stacked Area Chart was created using the following, rather complex, query as a demonstration.<br>
It aggregates all unlocked achievements from the last three Xbox generations and displays them on a timeline. Additionally, it performs fuzzy data cleanup to ensure no achievements are shown before the purchase of my very first Xbox 360.

```
let x360purchase = datetime(2009-04-01);
let xonepurchase = datetime(2014-09-01);
 
let calculated = achievements 
	| join (titles) on TitleId
 	| where IsUnlocked
	| extend unlocked = iff(datetime_part('year', TimeUnlocked) == 2005 and TimeUnlocked < Xbox360ReleaseDate, Xbox360ReleaseDate, TimeUnlocked)
 	| extend month = todatetime(format_datetime(unlocked, 'yyyy-MM-01'))
 	| summarize Gamerscore = sum(Gamerscore) by month, Platform; 

let sumlow = toscalar(calculated | where month < x360purchase | summarize sum(Gamerscore)); 
let months = range month from x360purchase to xonepurchase step 1d where dayofmonth(month) == 1;
let countmonths = toscalar(months | summarize count());
let projected = months | project month, Gamerscore = sumlow / countmonths, Platform = "Xbox360";
let mid = calculated | where month between (x360purchase .. xonepurchase) | project month, Gamerscore, Platform; 
let high = calculated | where month > xonepurchase | project month, Gamerscore, Platform; 
let midext = projected | join kind=leftouter (mid) on month, Platform | project month, Gamerscore = Gamerscore + coalesce(Gamerscore1,0), Platform;

union midext, high | order by month asc | render stackedareachart
```
