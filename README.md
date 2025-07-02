[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://buymeacoffee.com/spreed)

CS2 ADR Tracker
===============

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![Avalonia](https://img.shields.io/badge/UI-Avalonia-0098c7?logo=avalonia&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey?logo=windows&logoColor=white)
![Tests: NUnit](https://img.shields.io/badge/tests-NUnit-464646?logo=nunit&logoColor=white)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

A desktop application written in C# using WPF for tracking and analyzing your ADR (Average Damage per Round) in Counter-Strike 2 matches.
<br/>
The application allows you to manually enter your ADR score and the outcome of each game, then processes the data to help you evaluate your overall performance.

## Features

- **Manual ADR Input**: Enter the ADR score and outcome (win/loss/draw) for each match.
- **Overall ADR Calculation**: Automatically calculates and displays your overall ADR across all tracked matches.
- **Last 10 Games ADR**: Displays the average ADR of your last 10 games for quick trend analysis.
- **Game Results Summary**: Shows a count of all games, along with the count of wins and losses.
- **Simple WPF UI**: An intuitive and responsive interface built with WPF for easy input and tracking.

## Usage

1. Launch the application.
2. Enter your ADR score and select the match outcome (win/loss/draw).
3. Click "Add" to save the match.
4. View your overall and last 10 games ADR, as well as win/loss statistics.

## Screenshots

| ![](Screenshots/1.png) | ![](Screenshots/2.png) | ![](Screenshots/3.png) |
|-------------------------------|-------------------------------|-------------------------------|


## Technologies Used

- .NET 9
- Avalonia (for cross-platform UI)
- CommunityToolkit.Mvvm
- Serilog (logging)
- SQLite (via Dapper ORM)

## Enjoying this?
Just star the repo or make a donation.

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://buymeacoffee.com/spreed)

Your help is valuable since this is a hobby project for all of us: we do development during out-of-office hours.

## Support

If you encounter any issues or have questions, please open an [issue](https://github.com/spreedated/cs2adrtracker/issues).

## Contribution
Pull requests are very welcome.

## Copyrights
CS2 ADR Tracker was initially written by **Markus Karl Wackermann**.

## License

MIT License. See [LICENSE](LICENSE) for details.

Made with ❤️ by Dante Wackermann.
