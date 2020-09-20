function countUnique(iterable) {
    return new Set(iterable).size;
}

function formatPower(x) {
    return x;
}

function createD3SvgObject(data, dataMinMax) {

    //console.log(data);
    //https://datacadamia.com/viz/d3/histogram#instantiation
    //http://bl.ocks.org/nnattawat/8916402

    var svgTest = d3.select("#my_dataviz");
    svgTest.selectAll("*").remove();

    // set the dimensions and margins of the graph
    var margin = { top: 10, right: 65, bottom: 30, left: 10 },
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

    //console.log(Object.getOwnPropertyNames(data));
    const dataArray = Object.entries(data);
    const dataMinMaxArray = Object.entries(dataMinMax);
    //console.log(dataMinMaxArray);
    // Add Min & Max Area Range
    const dataArrayMinMaxOnHallOfFameBallot = dataMinMaxArray.filter(seasonPrediction => (seasonPrediction[1].algorithm == "OnHallOfFameBallot"));
    const dataArrayMinMaxInductedToHallOfFame = dataMinMaxArray.filter(seasonPrediction => (seasonPrediction[1].algorithm == "InductedToHallOfFame"));

    // Add one to the max season played for chart clarity
    var maxSeasonPlayed = d3.max(dataArray, d => d[1].seasonPlayed);

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
        .attr("transform", "translate(295,0)")
        .call(d3.axisRight(y));

    svg.append("path")
        .datum(dataArrayMinMaxInductedToHallOfFame)
        .style("opacity", .4)
        .attr("fill", "#9fbfdf")
        .attr("d", d3.area()
            .x(function (d) { return x(d[1].seasonPlayed) })
            .y0(function (d) { return y(d[1].min) })
            .y1(function (d) { return y(d[1].max) })
            .curve(d3.curveMonotoneX)
    )

    svg.append("path")
        .datum(dataArrayMinMaxOnHallOfFameBallot)
        .style("opacity", .4) 
        .attr("fill", "#dfbf9f")
        .attr("d", d3.area()
            .x(function (d) { return x(d[1].seasonPlayed) })
            .y0(function (d) { return y(d[1].min) })
            .y1(function (d) { return y(d[1].max) })
            .curve(d3.curveMonotoneX)
    )


    // Add points - OnHallOfFame
    svg.append('g')
        .selectAll("dot")
        .data(dataArray)
        .enter()
        .append("circle")
        .attr("cx", function (d) { return x(d[1].seasonPlayed); })
        .attr("cy", function (d) { return y(d[1].onHallOfFameBallotProbability); })
        .attr("r", 3)
        .style("fill", "#cc9966")
        .style("opacity", .5);

    // Add points - Inducted
    svg.append('g')
        .selectAll("dot")
        .data(dataArray)
        .enter()
        .append("circle")
        .attr("cx", function (d) { return x(d[1].seasonPlayed); })
        .attr("cy", function (d) { return y(d[1].inductedToHallOfFameProbability); })
        .attr("r", 3)
        .style("fill", "#6699cc")
        .style("opacity", .5);

    var selectedItems = [];
    for (i = 0; i != dataArray.length; i++) {
        //console.log(dataArray[i]);
        if (dataArray[i][1].algorithm == "StackedEnsemble") {
            console.log(dataArray[i]);
            selectedItems.push(dataArray[i]);
        };
    };

    // Add line - Inducted
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

    // Add line - OnHallOfFame
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

    // Add X axis label:
    svg.append("text")
        .attr("text-anchor", "middle")
        .attr("x", width / 2)
        .attr("y", height + margin.bottom / 1.25)
        .text("Season Played")
        .style("font-size", "10px")
        .style("font-weight", "bold");

    // Y axis label:
    svg.append("text")
        .attr("text-anchor", "middle")
        .attr("y", -(width+25))
        .attr("x", height/2)
        .attr("transform", "rotate(90)")
        .text("Probability")
        .style("font-size", "10px")
        .style("font-weight", "bold");
}