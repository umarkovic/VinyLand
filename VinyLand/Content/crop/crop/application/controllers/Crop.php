<?php
defined('BASEPATH') OR exit('No direct script access allowed');

class Crop extends CI_Controller {

	/**
	 * Index Page for this controller.
	 *
	 * Maps to the following URL
	 * 		http://example.com/index.php/welcome
	 *	- or -
	 * 		http://example.com/index.php/welcome/index
	 *	- or -
	 * Since this controller is set as the default controller in
	 * config/routes.php, it's displayed at http://example.com/
	 *
	 * So any other public methods not prefixed with an underscore will
	 * map to /index.php/welcome/<method_name>
	 * @see https://codeigniter.com/user_guide/general/urls.html
	 */
	public function index()
	{
		$this->load->view('layouts/index');
	}
	public function upload()
	{
		# code...
		
		if(isset($_POST["image"]))
		{
			$data = $_POST["image"];

			
			$image_array_1 = explode(";", $data);

			$image_array_2 = explode(",", $image_array_1[1]);


			$data = base64_decode($image_array_2[1]);

			$imageName = time() . '.png';

			file_put_contents($imageName, $data);

			echo '<img src="'.$imageName.'" class="avatar img-circle img-thumbnail" />';

		}
	}
}
