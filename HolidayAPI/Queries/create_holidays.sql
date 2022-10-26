CREATE TABLE holidays (
	country NVARCHAR(3) not null,
	region nvarchar(3),
	year INT not null, 
    month INT not null,
    holidayName NVARCHAR(200)
);