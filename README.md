# Real Estate Services

## Overview

Real Estate Services is a RESTful Web API built using .NET Core 8, designed to manage real estate listings and associated photos. The project follows the Onion architecture, ensuring a clean separation of concerns and promoting testability and maintainability. The API includes functionalities for adding, updating, archiving, and retrieving real estate listings, along with handling associated photos. It also features logging with Serilog and API documentation with Swagger.

## Features

- **Real Estate Listings Management**
  - Add new real estate listings.
  - Update listing details and timestamps.
  - Archive listings.
  - Retrieve all listings with sorting by the last updated time.

- **Photo Management**
  - Upload photos for real estate listings.
  - Delete photos associated with listings.
    
- **Lazy Loading**
  - Automatic loading of related data only when accessed, enhancing performance.

- **Logging**
  - Integrated with Serilog for comprehensive request and error logging.

- **API Documentation**
  - Swagger UI for interactive API documentation and testing.

## Getting Started

### Prerequisites

- .NET Core 8.0 SDK
- SQL Server (or any other compatible database)
- Visual Studio or any other IDE supporting .NET Core

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/amirhosseinmirmohammad/RealEstateAPI.git

 ### Create Database
 - cd RealEstateInfrastructure Project
 - dotnet ef migrations add InitialCreate
 - dotnet ef database update
