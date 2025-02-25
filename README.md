# Space-based Architecture Project 🚀

## Space-based Architecture Explanation 🏗️

**Space-based Architecture** is a scalable and flexible architecture designed to manage distributed systems. It is especially suitable for systems that need to process large volumes of data and requests and must provide optimal performance at scale.

In this architecture, a shared space (usually an In-Memory Data Grid or cache) is used to store data. This space, often referred to as the **space**, temporarily holds data. Then, after specific time intervals or when more processing is needed, the data is transferred to a permanent database (e.g., SQL Server).

One of the key features of this architecture is that data is simultaneously stored in both the cache and the database. This enables the system to respond to requests quickly, as data is first stored in the cache for fast access. Periodic synchronization ensures that the data is reliably and consistently stored in the permanent database.

---

### Key Features of Space-based Architecture ✨:

- **High Scalability**: By using cache and partitioning data across multiple nodes, this architecture can easily scale.
- **High Performance**: Data is temporarily stored in the cache, allowing for fast read and write operations.
- **Data Distribution**: Data is distributed across multiple servers and nodes, improving availability and fault tolerance.
- **Data Synchronization**: Periodic synchronization of data from cache to the database using message brokers like Kafka ensures data consistency without overloading the database.

This architecture is particularly useful in scenarios where high scalability, fast processing, and performance are critical, such as in financial systems, big data processing systems, and any system requiring high availability and scalability.

---

## Overview 📊

![Architecture Overview](https://github.com/user-attachments/assets/007965c0-9e2a-4144-8ab5-d05ff55993ad)

---

## Architecture Characteristics Ratings 🌟

![Architecture Ratings](https://github.com/user-attachments/assets/db31761e-05f6-4d8f-bdae-2cfd62a20d31)

---

## Project Components 🛠️

The system is composed of three main components:

1. **Identity**: Responsible for managing users, including creation and storage of user information.
2. **Catalog**: Manages product information such as names, prices, and availability.
3. **Order**: Handles processing and management of customer orders.

---

## System Workflow 🔄

1. Data is initially stored in a **cache** (Redis) for faster reading and writing.
2. After a specified period, the data is synchronized from the cache to the **database** (SQL Server) using **Kafka** as a **message broker**. This asynchronous synchronization ensures minimal load on the database.

---

## Technologies Used ⚙️

- **SQL Server**: Used for persistent database storage.
- **MediatR**: Facilitates the communication and flow of requests and responses within the system.
- **Redis**: Acts as a cache for faster data processing.
- **Kafka**: Serves as the message broker for synchronizing data between the cache and the database.
- **Quartz**: Handles task scheduling for periodic data synchronization.

---

## Advantages of This Architecture 🌟

- **High Scalability**: The system can easily scale by leveraging cache and partitioning data across multiple nodes.
- **Fast Performance**: Data is cached for quick access, and synchronization happens asynchronously to prevent database overload.
- **Reduced Database Load**: Kafka minimizes database load, leading to better overall performance and system reliability.

---

## Project Setup 🔧

To set up and run the project, follow these steps:

1. Install **Redis** and **SQL Server**.
2. Set up **Kafka** for message-based communication.
3. Configure **Quartz** for scheduling tasks (e.g., periodic data synchronization).
4. Run the project on **.NET Core** or another compatible platform.

---

## How to Use the Project ⚡

1. **Create User**: Navigate to the **Identity** component to register a new user.
2. **Manage Products**: Go to the **Catalog** component to view or manage product details.
3. **Place an Order**: Access the **Order** component to place a customer order.
4. The system will automatically synchronize data to the database after the specified period.

---

### 🏁 Ready to Go! 🚀

Feel free to explore, contribute, and expand on this project! For any questions, feel free to contact us at [heydarabadip@gmail.com].

---

### Contributing 🤝

We welcome contributions! If you find any issues or have ideas to improve the project, please submit a pull request or open an issue. Let’s build something amazing together! 🌟
