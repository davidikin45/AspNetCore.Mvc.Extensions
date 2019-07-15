const FetchStreaming = (function () {

    let table;
    let fetchButton, fetchStreamButton;

    function initializeUI() {
        fetchButton = document.getElementById('fetch');
        fetchButton.addEventListener('click', fetchData);

        fetchStreamButton = document.getElementById('fetch-stream');
        fetchStreamButton.addEventListener('click', fetchDataStream);

        table = document.getElementById('table');
    }

    function fetchData() {
        clearTable();

        fetch('api/Collection')
            .then(function (response) {
                return response.json();
            })
            .then(function (fetchItems) {
                fetchItems.forEach(appendRow);
            });
    }

    function fetchDataStream() {
        clearTable();

        fetch('api/Collection/stream')
            .then(function (response) {
                const fetchItems = response.body
                    .pipeThrough(new TextDecoderStream())
                    .pipeThrough(parseNDJSON());

                readStream(fetchItems.getReader());
            });
    }

    function clearTable() {
        for (let rowIndex = 1; rowIndex  < table.rows.length;) {
            table.deleteRow(rowIndex );
        }
    }

    function parseNDJSON() {
        let ndjsonBuffer = '';

        return new TransformStream({
            transform: function(ndjsonChunk, controller) {
                ndjsonBuffer += ndjsonChunk;

                const jsonValues = ndjsonBuffer.split('\n');
                jsonValues.slice(0, -1).forEach(function (jsonValue) { controller.enqueue(JSON.parse(jsonValue)); });

                ndjsonBuffer = jsonValues[jsonValues.length - 1];
            },
            flush: function(controller) {
                if (ndjsonBuffer) {
                    controller.enqueue(JSON.parse(ndjsonBuffer));
                }
            }
        });
    }

    function readStream(streamReader) {
        streamReader.read()
            .then(function (result) {
                if (!result.done) {
                    appendRow(result.value);

                    readStream(streamReader);
                }
            });
    }

    function appendRow(fetchItem) {
        let newRow = table.insertRow(-1);

        newRow.insertCell(0).appendChild(document.createTextNode(fetchItem.dateFormatted));
        newRow.insertCell(1).appendChild(document.createTextNode(fetchItem.temperatureC));
        newRow.insertCell(2).appendChild(document.createTextNode(fetchItem.temperatureF));
        newRow.insertCell(3).appendChild(document.createTextNode(fetchItem.summary));
    }

    return {
        initialize: function () {
            initializeUI();
        }
    };
})();

FetchStreaming.initialize();