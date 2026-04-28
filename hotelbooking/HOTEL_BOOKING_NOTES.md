# Hotel Booking System Notes

## What This Project Does

This project models a simple hotel room booking system using core object-oriented design.
It lets a guest book one or more rooms for a date range, prevents overlapping bookings for the same room, and supports cancellation with a 24-hour refund rule.

The code is intentionally small and interview-friendly. It shows the main OOP ideas without using a heavy framework or advanced architecture.

## Main Classes

### `Hotel`

`Hotel` is the central object that owns the room inventory and the reservation list.

Responsibilities:
- Store the hotel name.
- Store all available rooms.
- Create new reservations.
- Cancel reservations.
- Prevent double booking.

Why this matters:
- The hotel is the natural place to enforce booking rules because it owns availability.
- This keeps booking logic in one place instead of spreading it across multiple classes.

### `Room` and Room Types

`Room` is an abstract base class.

Concrete room types:
- `SingleRoom`
- `DoubleRoom`
- `SuiteRoom`

Each room has:
- A room number.
- A nightly rate.
- A room type label.

Why inheritance is used:
- Each room type shares the same basic data and behavior.
- The subclasses only change the price and type name.
- This keeps shared behavior in one place and avoids duplication.

### `Guest`

`Guest` stores customer details such as:
- Name
- Email

This class is simple because the project scope is intentionally small.

### `Reservation`

`Reservation` represents one booking made by a guest.

It stores:
- Reservation ID
- Guest
- Check-in date
- Check-out date
- Rooms included in the reservation
- Reservation status

Responsibilities:
- Validate the date range.
- Calculate total nights.
- Calculate total price.
- Check if a date range overlaps.
- Mark itself as cancelled.

### `ReservationStatus`

An enum that tracks whether a reservation is:
- `Active`
- `Cancelled`

### `CancellationResult`

This is a small result object returned after cancellation.

It tells you:
- Which reservation was cancelled.
- Whether the cancellation is refundable.
- The refund amount.

## Booking Flow

When `Hotel.BookRooms(...)` is called, the following happens:

1. The requested room numbers are validated.
2. Each room number is matched against the hotel inventory.
3. The hotel checks whether any of those rooms already has an active reservation that overlaps the requested dates.
4. If a conflict exists, booking fails with an exception.
5. If all rooms are available, a new `Reservation` is created.
6. The reservation is stored inside the hotel.
7. The new reservation is returned to the caller.

This is a clean example of encapsulation because the caller does not manipulate the room list directly.

## How Double Booking Is Prevented

Double booking is prevented inside `Hotel`.

The important rule is:
- A room cannot be booked again if it already has an active reservation whose date range overlaps the new booking.

The overlap check uses a standard half-open interval idea:
- Check-in is treated as inclusive.
- Check-out is treated as exclusive.

In plain language:
- If one booking ends on the same day another booking starts, that is allowed.
- If the dates truly overlap, the booking is rejected.

This logic is kept in one method so the rule is easy to find and maintain.

## Cancellation Rule

The project rule says:
- Cancellations made within 24 hours of check-in are non-refundable.

How it works:
1. The reservation is found by ID.
2. The reservation checks whether the cancellation time is earlier than the 24-hour cutoff.
3. If the cancellation is before that cutoff, it is refundable.
4. If it is too close to check-in, it is non-refundable.
5. The reservation is marked as cancelled.
6. A `CancellationResult` is returned.

Important note:
- In this demo, the example uses `DateTime` values directly.
- That means the app assumes a simple time model rather than real-world time zone handling.

## Design Principles Used

### Encapsulation

Each class hides its internal state and exposes only the operations that make sense.

Examples:
- `Hotel` manages its private reservation list.
- `Reservation` hides its mutable room collection.
- `Room` exposes read-only properties.

### Abstraction

The abstract `Room` class defines the common shape for all room types.

This lets the rest of the system work with `Room` instead of caring about the exact subtype.

### Inheritance

`SingleRoom`, `DoubleRoom`, and `SuiteRoom` inherit from `Room`.

This is used only where the behavior is genuinely shared.

### Polymorphism

The system can treat all rooms as `Room` objects, but each subtype supplies its own type label and nightly rate.

That makes it easy to extend the system later with new room categories.

### SOLID

- Single Responsibility: each class has one main job.
- Open/Closed: new room types can be added without changing the booking flow.
- Liskov Substitution: any room subtype can be used where a `Room` is expected.
- Interface Segregation: the project does not force unnecessary interfaces.
- Dependency Inversion: this project is small, so the simplest direct model is used instead of over-abstracting.

### KISS

The design stays small and direct.

There is no repository layer, no service layer, and no dependency injection container because they are not needed for this scope.

### DRY

Common validation and shared room behavior live in base classes or shared methods.

## Console Demo in `Program.cs`

The console app demonstrates the system in a simple script-like flow:

1. Create a hotel with a few rooms.
2. Create a guest.
3. Book multiple rooms in a single reservation.
4. Print the reservation details and total price.
5. Try to book the same room again for the same dates.
6. Show that double booking is blocked.
7. Cancel one reservation before the refund cutoff.
8. Cancel another reservation within the 24-hour window.
9. Show the different refund outcomes.

This makes the project easy to read in an interview because the output directly matches the business rules.

## Current Scope Limits

This project is intentionally small.

It does not include:
- Persistence to a database.
- User login.
- Payment processing.
- Room search filters.
- Real time zone handling.
- Calendar UI.

That is by design. For an entry-level OOP interview, the goal is to show clean modeling and correct business rules rather than full product complexity.

## Good Interview Talking Points

If you are asked to explain the design, you can say:

- The `Hotel` class owns booking rules and room availability.
- `Room` is abstract because all room types share the same base identity and pricing model.
- The system prevents double booking by checking overlap against active reservations.
- Cancellation is handled centrally and returns a result object so the caller can see refund status.
- The design is intentionally simple to keep it readable and easy to extend.

## How To Extend It Later

If you want to grow this project, the next natural improvements are:
- Add tests for overlapping booking and cancellation.
- Add persistence with a repository.
- Add a price calculator service for discounts or taxes.
- Add a search function for available rooms by type.
- Add check-in and check-out workflows.
