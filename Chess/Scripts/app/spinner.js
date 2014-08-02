function CreateSpinner() {
    return new Spinner({
        top: "100%",
        left: "100%",
        lines: 12, // The number of lines to draw
        length: 7, // The length of each line
        width: 5, // The line thickness
        radius: 10, // The radius of the inner circle
        color: '#B57271', // #rbg or #rrggbb
        speed: 1, // Rounds per second
        trail: 100, // Afterglow percentage
        shadow: false // Whether to render a shadow
    });
}