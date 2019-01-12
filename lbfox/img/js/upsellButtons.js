$(DocumentHelper.getDocument()).ready(function() {
    nmvtisButtonSetup();
    canadaLienButtonSetup();

    $('#runLienReportBtn').click(function() {
        displayPending();
    });
});

function nmvtisButtonSetup() {
	if(setDisplayBasedOnPreviousNmvtisPurchase) {
		if($('#nmvtisResultsBtn').length > 0) {
			setDisplayBasedOnPreviousNmvtisPurchase(meta('subscriberVin'), function(nmvtisCheck){
				$('#nmvtisResultsBtn').text(nmvtisCheck.id == 0 ? 'Run' : 'View');
			});
		}
	}
}

function canadaLienButtonSetup() {
    if(setDisplayBasedOnLienReportInventory) {
        if($('#runLienReportBtn').length > 0) {
            setDisplayBasedOnLienReportInventory(meta('subscriberVin'), function(data){
                if (data.vinStatuses[0].status === 'Complete') {
                    displayComplete(data.vinStatuses[0].reportUrl);
                } else if(data.vinStatuses[0].status === 'Pending') {
                    displayPending();
                }
            });
        }
    }
}

function displayComplete(url) {
    $('#runLienReportBtn').text('Buy Update');
    $('#viewLienReportBtn').attr('href', url)
    $('#viewLienReportBtn').show();
}

function displayPending() {
    $('#vhrLienButtons').hide();
    $('#vhrLienPending').css('display', 'inline-block')
}