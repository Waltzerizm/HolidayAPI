# HolidayAPI

[Web Api](https://holidaydb.ew.r.appspot.com/swagger/index.html)

The application was developed on Windows OS using .NET6.0 ASP.NET Core Web API.

The Web Api and database is deployed on Google Cloud. 

## Deployment

To deploy the application on your local machine, clone this repository and compile it using a dotnet compiler (ideally compile using Visual Studio 2022). This should open localhost:7274/swagger/index/html and you're ready to go to use the API. 

If you wish to use your own database, make sure to change the ConnectionString in appsettings.json and create the required tables, which SQL create statements can be located in Queries/{table_name}.sql

## Comments due to the application

Theres quite a few warnings due to possible null references, which hopefully I will be able to patch up later on in the week. (Unfortunatly, after the deadline :-( )

I did not have time to sterialize User input and test it out really well, but hopefully it should work well!

The database table structures and namings are a mess, thats something that would be needed to fix asap. 

I couldn't host the application on Azure, since I was no longer eligible for the free trail, that's why I chose Google Cloud.

GetDayStatus and GetMaxFreeday API calls return [{variable: value}] or {variable:value} depending if it's queried from the API or database (whoops).

If you have any questions or regards, feel free to contact me :-) Gmail- rokas.lekecinskas@gmail.com or Facebook/LinkedIn - Rokas Lekečinskas.
