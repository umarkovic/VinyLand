<html>  
    <head>  
        <title>Croping </title>  
		<script src="assets/jquery.min.js"></script>  
		<script src="assets/bootstrap.min.js"></script>
		<script src="assets/croppie.js"></script>
		<link rel="stylesheet" href="assets/bootstrap.min.css" />
		<link rel="stylesheet" href="assets/croppie.css" />
    </head>  
    <body>  
        <div class="container">
          <br />
      <h3 align="center">Crop Image before upload in bootstrap model with Codeigniter, PHP, Mysql and jQuery</h3>
      <br />
      <br />
  				<div class="panel-body" align="center">
  					 <div id="uploaded"><img src="http://ssl.gstatic.com/accounts/ui/avatar_2x.png" class="avatar img-circle img-thumbnail" alt="avatar"></div>
            <input type="file" name="upload" id="upload" />
  					<br />
  				

  		</div>
    </body>  
</html>

<div id="myModal" class="modal" role="dialog">
	<div class="modal-dialog">
		<div class="modal-content">
      		<div class="modal-header">
        		<button type="button" class="close" data-dismiss="modal">&times;</button>
        		<h4 class="modal-title">Upload & Crop Image</h4>
      		</div>
      		<div class="modal-body">
        		<div class="row">
  					<div class="col-md-8 text-center">
						  <div id="image" style="width:250px; margin-top:20px"></div>
  					</div>
  					<div class="col-md-4" style="padding-top:20px;">
  						<br />
  						<br />
  						<br/>
						  <button class="btn btn-success crop_image">Crop & Upload Image</button>
					</div>
				</div>
      		</div>
      		<div class="modal-footer">
        		<button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
      		</div>
    	</div>
    </div>
</div>

<script>  
$(document).ready(function(){

	$image_crop = $('#image').croppie({
    enableExif: true,
    viewport: {
      width:200,
      height:200,
      type:'square' //circle
    },
    boundary:{
      width:300,
      height:300
    }
  });

  $('#upload').on('change', function(){
    var reader = new FileReader();
    reader.onload = function (event) {
      $image_crop.croppie('bind', {
        url: event.target.result
      }).then(function(){
        console.log('jQuery bind complete');
      });
    }
    reader.readAsDataURL(this.files[0]);
    $('#myModal').modal('show');
  });



  $('.crop_image').click(function(event){
    $image_crop.croppie('result', {
      type: 'canvas',
      size: 'viewport'
    }).then(function(response){
      $.ajax({
        url:'<?php echo base_url(); ?>crop/upload',
        type: "POST",
        data:{"image": response},
        success:function(data)
        {
          $('#myModal').modal('hide');
          $('#uploaded').html(data);
        }
      });
    })
  });

});  
</script>