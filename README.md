# Persistenr Register Project

## Overview

The .Net Core Web API project  revolves around a simple User Registration system. The system consists of a background service that receives User Registration related data through Http requests. Then it persists the data in two different data stores, in this case an SQL Server DB and a Json file. Then it reposts the data (enriched with some extra details) in 2 other EndPoints. The purpose of this project is to demonstrate some patterns and techniques, so it's not a full implemented solution regarding the business logic. Patterns and technologies used include: Repository Pattern, Service Layer, MVC, Dependency Injection, Inheritance, Interfaces, AutoMapper, Retry Logic, Logging, Saga Pattern, Entity Framework, DTOs, SOLID Principles. The project was written in Visual Studio Code with net7.0.

## Design
The project consists of 4 solutions "PersistentRegister", "SecondEndPoint", "ThirdEndPoint" and a class library called "ApiLibrary", which has some constants (used for Messages) and classes that are common among the rest of the solutions. The main one (PersistentRegister) is the one responsible for receiving the initial data, persists them and then sends them to the other EndPoints. The last two are identical and implement the minimum functionality of receiving and handling "Insert" and "Delete" requests. I prefered that design so I could use different ports and test the whole functionality by having different services running each time. The library is just referenced by all other 3 solutions.

The flow starts with "PersistentRegister" receiving the data. Then it will try to store them in a local SQL Server DB (created with Entity Framework) and a local json file (created with Newtonsoft). Then it will map the data to an enriched model that has some extra persist details and it will try to send them to the other 2 EndPoints. If any of the aftermentioned actions fail, then the system will rollback any changes that have been made. Regarding the transaction with the EndPoints, the main service will try to send the data up to 5 times (according to the retry policy that has been defined).

During the whole lifecycle the Serilog logs some information in local .txt files (a new one every day according to the configuartion).

## Features

- **MVC Architecture**: The project follows the MVC pattern, providing a structured and modular approach to design, making it easier to manage and scale. The Controllers have a pretty simple functionality, they just called the appropriate service and do nothing more. The Models have only the Classes with their fields, with no business logic.

- **Services**: Encapsulates business logic into services. This allows for separation of concers.

- **Repository Pattern**: Encapsulates the logic for data access and persistence in a dedicated repository layer. Only the Repositories have access to the DbContext and any DB transactions.

- **Dependency Injection (DI)**: Utilizes DI to promote loose coupling and improve the maintainability of the code. This allows for easy integration of new components and services.

- **Entity Framework (EF)**: Uses EF for all the database interactions and follows the code-first logic.

- **DTOs (Data Transfer Objects)**: Utilizes DTOs to seperate the API layer and the underlying business logic, enhancing flexibility and security. In this project there wasn't really any need to do that, since the Models are really simple, but it is done for demonstration purposes.

- **AutoMapper**: Used for the mapping between the Models and the DTOs.

- **Polly**: Used in order to establish Retry Policies when sending the events to the 

-**Serilog**: Used in order to keep logs of key actions throught the whole lifecycle.

- **SOLID Principles**:

    The project also follows the SOLID principles up to a point.

  - **Single Responsibility Principle (SRP)**: The use of Dependency Injection, services, and DTOs promote modularity and seperation of concerns.
  
  - **Open/Closed Principle (OCP)**: The use of Dependency Injection and services allows extensibility without modifying existing code.

  - **Liskov Substitution Principle (LSP)**: With the current requirements there wasn't really any opportunity to implement it.

  - **Interface Segregation Principle (ISP)**: Even though interfaces have been used, it needed a project of a larger scale to demonstrate properly.

  - **Dependency Inversion Principle (DIP)**: The utilization of Dependency Injection is an implementation of the DIP.

## How to run it

1. **Prerequisites**: Ensure that you have .NET Core SDK and Docker installed on your machine.

2. **Clone the Repository**: Clone this repository to your local machine using the following command:
    ```bash
    git clone https://github.com/Fenrif/PersistentRegister.git
    git clone https://github.com/Fenrif/SecondEndPoint.git
    git clone https://github.com/Fenrif/ThirdEndPoint.git
    ```
    The ApiLibrary wasn't published, as the .dll is referenced directly in each project.

3. **Database Setup**: Update the connection string in the `appsettings.json` file to point to your database. Run the following commands to apply migrations:
    ```bash
    dotnet ef database update
    ```

5. **Explore the API**: Access the PeristentRegister API at `http://localhost:5196` or `https://localhost:7245`, the second EndPoint at `https://localhost:7175` or `http://localhost:5241` and the ThirdEndPoint at `https://localhost:7296` or `http://localhost:5220`. Use tools like Postman or Swagger to make HTTP requests. 

