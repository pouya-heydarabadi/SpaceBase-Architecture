# Space-based Architecture Project 🚀

## Space-based Architecture Explanation 🏗️

**Space-based Architecture** is a scalable and flexible architecture designed to manage distributed systems. It is especially suitable for systems that need to process large volumes of data and requests and must provide optimal performance at scale.

In this architecture, a shared space (usually an In-Memory Data Grid or cache) is used to store data. This space, often referred to as the **space**, temporarily holds data. Then, after specific time intervals or when more processing is needed, the data is transferred to a permanent database (e.g., SQL Server).

One of the key features of this architecture is that data is simultaneously stored in both the cache and the database. This enables the system to respond to requests quickly, as data is first stored in the cache for fast access. Periodic synchronization ensures that the data is reliably and consistently stored in the permanent database.

### Key Features of Space-based Architecture ✨:

- **High Scalability**: By using cache and partitioning data across multiple nodes, this architecture can easily scale.
- **High Performance**: Data is temporarily stored in the cache, allowing for fast read and write operations.
- **Data Distribution**: Data is distributed across multiple servers and nodes, improving availability and fault tolerance.
- **Data Synchronization**: Periodic synchronization of data from cache to the database using message brokers like Kafka ensures data consistency without overloading the database.

This architecture is particularly useful in scenarios where high scalability, fast processing, and performance are critical, such as in financial systems, big data processing systems, and any system requiring high availability and scalability.

## Overview
![image](https://github.com/user-attachments/assets/007965c0-9e2a-4144-8ab5-d05ff55993ad)

## Architecture Characteristics Ratings

![image](https://github.com/user-attachments/assets/db31761e-05f6-4d8f-bdae-2cfd62a20d31)


---

## Project Components 🛠️

The system is composed of three main components:

1. **Identity**: This component is responsible for managing users. It handles the creation and storage of user information.
2. **Catalog**: This component manages the products. The catalog stores details such as product names, prices, and availability.
3. **Order**: This component is responsible for processing orders. It records details about the products selected by customers.

---

## System Workflow 🔄

1. Initially, data is stored in a **cache** (Redis) to ensure faster reading and writing operations.
2. After a specified period, the data is synchronized from the cache to the database (SQL Server) using **Kafka** as a **message broker**. This synchronization happens asynchronously to reduce the load on the database.

---

## Technologies Used ⚙️

- **SQL Server**: Used for persistent storage of the database.
- **MediatR**: Handles the communication and flow of requests and responses within the system.
- **Redis**: Utilized as a cache for faster data access and processing.
- **Kafka**: Acts as the message broker to synchronize data between the cache and the database.
- **Quartz**: Used for task scheduling and executing regular operations, such as syncing data periodically.

---

## Advantages of This Architecture 🌟

- **High Scalability**: The use of cache and message brokers allows for easy scaling of the system.
- **Fast Performance**: Data is stored in the cache for quicker access, and synchronization occurs asynchronously to minimize database load.
- **Reduced Database Load**: With Kafka, the database load is reduced, increasing overall system performance and reliability.

---

## Project Setup 🔧

1. Install **Redis** and **SQL Server**.
2. Set up **Kafka** for message-based communication.
3. Configure **Quartz** for task scheduling (e.g., periodic data syncing).
4. Run the project using **.NET Core** or any suitable platform.

---

## How to Use the Project ⚡

1. To create a new user, navigate to the **Identity** component and register the user.
2. To view or manage products, visit the **Catalog** component.
3. To place an order, go to the **Order** section.
4. The system will automatically sync the data to the database after a specified period.

---

### 🏁 Ready to Go! 🚀

Feel free to explore, contribute, and expand on this project! For any questions, contact us at [heydarabadip@gmail.com].
