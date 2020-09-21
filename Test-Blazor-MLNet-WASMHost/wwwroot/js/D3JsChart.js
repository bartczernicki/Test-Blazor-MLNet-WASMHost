function createD3SvgObject(data, dataMinMax, selectedAlgorithm) {

    //console.log(data);
    console.log(selectedAlgorithm);

    //https://datacadamia.com/viz/d3/histogram#instantiation
    //http://bl.ocks.org/nnattawat/8916402

    // Convert set of objects to arrays
    const dataArray = Object.entries(data);
    const dataMinMaxArray = Object.entries(dataMinMax);
    //console.log(dataMinMaxArray);
    // Create Min & Max Area Range array for simplicity
    const dataArrayMinMaxOnHallOfFameBallot = dataMinMaxArray.filter(seasonPrediction => (seasonPrediction[1].algorithm == "OnHallOfFameBallot"));
    const dataArrayMinMaxInductedToHallOfFame = dataMinMaxArray.filter(seasonPrediction => (seasonPrediction[1].algorithm == "InductedToHallOfFame"));

    // Set max seasons played (max range)
    var maxSeasonPlayed = d3.max(dataArray, d => d[1].seasonPlayed);


    // DRAW D3 CHART - SVG

    // 1) Set up chart canvas
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

    // 2) Add X, Y Axis

    // Add - X axis
    var x = d3.scaleLinear()
        .domain([1, maxSeasonPlayed])
        .range([0, width]);
    svg.append("g")
        .attr("transform", "translate(0," + height + ")")
        .call(d3.axisBottom(x));
    // X axis label
    svg.append("text")
        .attr("text-anchor", "middle")
        .attr("x", width / 2)
        .attr("y", height + margin.bottom / 1.25)
        .text("Season Played")
        .style("font-size", "10px")
        .style("font-weight", "bold");

    // Add - Y axis
    var y = d3.scaleLinear()
        .domain([0, 1])
        .range([height, 0]);
    svg.append("g")
        .attr("transform", "translate(295,0)")
        .call(d3.axisRight(y));
    // Y axis label
    svg.append("text")
        .attr("text-anchor", "middle")
        .attr("y", -(width + 27))
        .attr("x", height / 2)
        .attr("transform", "rotate(90)")
        .text("Probability - " + selectedAlgorithm)
        .style("font-size", "10px")
        .style("font-weight", "bold");


    // 3) Area Range

    // Add - Area Range - Inducted
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

    // Add - Area Range - OnHallOfFameBallot
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

    // 4) Add Prediction Points

    // Add points - OnHallOfFameBallot
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


    // 5) Add Line for Selected Algorithm 

    var selectedItems = [];
    for (i = 0; i != dataArray.length; i++) {
        //console.log(dataArray[i]);
        if (dataArray[i][1].algorithm == selectedAlgorithm) {
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

    // 6) Add Legend (last so it is top-level)
    // Legend
    // Points
    svg.append("circle").attr("cx", 0).attr("cy", 0).attr("r", 5).style("fill", "#dfbf9f");
    svg.append("circle").attr("cx", 0).attr("cy", 12).attr("r", 5).style("fill", "#9fbfdf");
    svg.append("text").attr("x", 6).attr("y", 1).text("Hall of Fame - On Ballot")
        .style("font-size", "8px").attr("alignment-baseline", "middle").style("font-weight", "bold");
    svg.append("text").attr("x", 6).attr("y", 13).text("Hall of Fame - Inducted")
        .style("font-size", "8px").attr("alignment-baseline", "middle").style("font-weight", "bold");
    // Line
    //svg.append("rect").attr("x", -4).attr("y", 20)
    //    .attr("width", 9)
    //    .attr("height", 3)
    //    .style("fill", "#cc9966");
    //svg.append("rect").attr("x", -4).attr("y", 24)
    //    .attr("width", 9)
    //    .attr("height", 3)
    //    .style("fill", "#6699cc");
    //svg.append("text").attr("x", 6).attr("y", 24).text("Stacked Ensemble")
    //    .style("font-size", "8px").attr("alignment-baseline", "middle").style("font-weight", "bold");
}