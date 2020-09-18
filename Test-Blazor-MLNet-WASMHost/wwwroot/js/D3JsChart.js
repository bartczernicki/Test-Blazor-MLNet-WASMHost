function countUnique(iterable) {
    return new Set(iterable).size;
}

function formatPower(x) {
    return x;
}

function createD3SvgObject(data, title) {

    console.log(data);
    //https://datacadamia.com/viz/d3/histogram#instantiation
    //http://bl.ocks.org/nnattawat/8916402

    var svgTest = d3.select("#my_dataviz");
    svgTest.selectAll("*").remove();

    // set the dimensions and margins of the graph
    var margin = { top: 10, right: 25, bottom: 30, left: 10 },
        width = 370 - margin.left - margin.right,
        height = 400 - margin.top - margin.bottom;

    // append the svg object to the body of the page
    var svg = d3.select("#my_dataviz")
        .append("svg")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
        .append("g")
        .attr("transform",
            "translate(" + margin.left + "," + margin.top + ")");

    console.log(typeof data);
    //console.log(Object.getOwnPropertyNames(data));
    const dataArray = Object.entries(data);
    console.log(dataArray);

    var maxSeasonPlayed = d3.max(dataArray, d => d[1].seasonPlayed) + 1;

    // Add X axis
    var x = d3.scaleLinear()
        .domain([1, maxSeasonPlayed])
        .range([0, width]);
    // Add Y axis
    var y = d3.scaleLinear()
        .domain([0, 1])
        .range([height, 0]);

    svg.append("g")
        .attr("transform", "translate(0," + height + ")")
        .call(d3.axisBottom(x));

    svg.append("g")
        .attr("transform", "translate(335,0)")
        .call(d3.axisRight(y));

    var tooltip = d3.select("#my_dataviz")
        .append("div")
        .style("opacity", 0)
        .attr("class", "tooltip")
        .style("background-color", "white")
        .style("border", "solid")
        .style("border-width", "1px")
        .style("border-radius", "5px")
        .style("padding", "10px");

    // tooltip mouseover event handler
    var tipMouseover = function (d) {

        //console.log(d);

        var html = "Season: " + d[1].seasonPlayed + "<br/>";// +
            //"<span style='color:" + color + ";'>" + d.manufacturer + "</span><br/>" +
            //"<b>" + d.sugar + "</b> sugar, <b/>" + d.calories + "</b> calories";

        console.log(html);

        tooltip.html(html)
            .style("left", (d3.event.pageX + 15) + "px")
            .style("top", (d3.event.pageY - 28) + "px")
            .transition()
            .duration(200) // ms
            .style("opacity", .9) // started as 0!

    };

    // tooltip mouseout event handler
    var tipMouseout = function (d) {
        tooltip.transition()
            .duration(300) // ms
            .style("opacity", 0); // don't care about position!
    };

    // Add dots
    svg.append('g')
        .selectAll("dot")
        .data(dataArray)
        .enter()
        .append("circle")
        .attr("cx", function (d) { return x(d[1].seasonPlayed); })
        .attr("cy", function (d) { return y(d[1].inductedToHallOfFameProbability); })
        .attr("r", 3)
        .style("fill", "#6699cc")
        .style("opacity", .6) 
        .on("mouseover", tipMouseover)
        .on("mouseout", tipMouseout);


    svg.append('g')
        .selectAll("dot")
        .data(dataArray)
        .enter()
        .append("circle")
        .attr("cx", function (d) { return x(d[1].seasonPlayed); })
        .attr("cy", function (d) { return y(d[1].onHallOfFameBallotProbability); })
        .attr("r", 3)
        .style("fill", "#cc9966")
        .style("opacity", .6) 
        .on("mouseover", tipMouseover)
        .on("mouseout", tipMouseout);


    var selectedItems = [];
    for (i = 0; i != dataArray.length; i++) {
        //console.log(dataArray[i]);
        if (dataArray[i][1].algorithm == "StackedEnsemble") {
            console.log(dataArray[i]);
            selectedItems.push(dataArray[i]);
        };
    };

    svg.append("path")
        .datum(selectedItems)
        .attr("fill", "none")
        .attr("stroke", "#6699cc")
        .attr("stroke-width", 1.5)
        .attr("d", d3.line() 
            .x(function (d) { return x(d[1].seasonPlayed) })
            .y(function (d) { return y(d[1].inductedToHallOfFameProbability) })
            .curve(d3.curveMonotoneX)
    )


    svg.append("path")
        .datum(selectedItems)
        .attr("fill", "none")
        .attr("stroke", "#cc9966")
        .attr("stroke-width", 1.5)
        .attr("d", d3.line()
            .x(function (d) { return x(d[1].seasonPlayed) })
            .y(function (d) { return y(d[1].onHallOfFameBallotProbability) })
            .curve(d3.curveMonotoneX)
        )
}