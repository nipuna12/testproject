# Chinook

# Playlist Management System

## Overview:
This project implements a playlist management system using Blazor for the front-end and Entity Framework Core for data access. The system allows users to add tracks to playlists, create new playlists, and manage their favorite tracks.

## Key Components

### Service Layer

PlaylistService: This service provides methods to interact with the database, such as retrieving playlists, creating new playlists, and adding tracks to playlists.


### Blazor Page

ArtistPage.razor
This page is focused on displaying artist information and tracks. It allows users to add tracks to their favorite playlists directly from the artist page.

PlalistPage.razor: TODO..............

The list of user playlists is dynamically updated after any changes, ensuring that the UI reflects the current state of the data.
Two-way data binding ensures that the form controls are always in sync with the model.

## Summary
This approach separates concerns effectively, with the service layer focused on data access and business logic, and the Blazor page handling user interactions and error display. This results in clean, maintainable code and a responsive user interface.