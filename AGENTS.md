# Project Context & Rules

## Overview:
- I'm building a very basic app to learn more about S3 buckets. I've built a skeleton ui to basically upload files and store them in S3, and have done so successfully. This is merely a testing project, so I don't need anything fancy, just bare bones functionality -- an MVP. My goal for users to be able to upload files and retrieve all their own files based on their id and the id that gets created when they upload a file to S3.

## Tech Stack
- **Frontend**: React-TypeScript
- **Backend**: .NET 10
- **Database**: SQL Server
- **File Storage**: S3 

## Frontend (client folder)
### Frontend Project Structure
- `/src/components`: UI components
- `/src/lib`: utlity/helper functions

### Frontend code style
- double quotes
- include semi-colon
- 4 space tabs
- use explicit types (only use any if necessary)

## Backend (server folder)
### Backend Project Structure
- follow Controller-Service-Repository pattern
- create interfaces for service and repository
- DO NOT implement any of the other functions (I will implement all of the S3 functionality for the sake of learning). Just create the models and database integrations.

### Database
- Database name: S3BucketTesting
- Server name: RICH-PC
- Connection: RICH-PC\horic

#### Models
- User: id, name, email, createdAt (or use .NET Identity, if easier)
- S3Items: id, userId, s3Key, uploadedAt, lastRetrieved, fileName, mimeType, fileSize