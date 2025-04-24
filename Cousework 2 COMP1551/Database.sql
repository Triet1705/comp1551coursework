CREATE DATABASE CourseworkCOMP1551;
USE CourseworkCOMP1551;
CREATE TABLE ProcessingLog (
    LogID INT PRIMARY KEY AUTO_INCREMENT,
    Timestamp DATETIME,
    InputString VARCHAR(40),
    ProcessorType VARCHAR(255),
    Parameter INT,
    OutputString TEXT
);
CREATE TABLE SavedResults (
    SavedResultID INT PRIMARY KEY AUTO_INCREMENT,
    ResultName VARCHAR(255),
    InputString VARCHAR(40),
    ProcessorType VARCHAR(255),
    Parameter INT,
    OutputString TEXT
);
CREATE TABLE ProcessorStats (
    ProcessorType VARCHAR(255) PRIMARY KEY,
    UsageCount INT
);