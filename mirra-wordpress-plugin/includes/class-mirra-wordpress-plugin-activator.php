<?php

/**
 * Fired during plugin activation.
 *
 * This class defines all code necessary to run during the plugin's activation.
 *
 * @since      1.0.0
 * @package    Mirra_Wordpress_Plugin
 * @subpackage Mirra_Wordpress_Plugin/includes
 */

class Mirra_Wordpress_Plugin_Activator {
	
	public static function generate_custom_password() {
	    $charset = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
	    $password = '';

	    for ($i = 0; $i < 24; $i++) {
	        if ($i > 0 && $i % 4 == 0) {
	            $password .= ' ';
	        }
	        $password .= $charset[random_int(0, strlen($charset) - 1)];
	    }

	    return $password;
	}



	/**
	 * Generates Password to post to this wordpress blog.
	 *
	 * Generates a safe password that will be sent to Mirra AI Server. This password will then be used to post content to this Wordpress instance.
	 *
	 * @since    1.0.0
	 */

	public static function create_custom_user_with_mirra_app_password() {
	
		if(!current_user_can('create_users') || !current_user_can('publish_posts'))
			return;

    	$username = 'mirra_user';
    	$password = self::generate_custom_password();
    	$email = 'mirra_user@email.com';

    	// Verifica se o usuário já existe
    	if (!username_exists($username) && !email_exists($email)) {
    	    $user_id = wp_create_user($username, $password, $email);

    	    // Adiciona uma role ao usuário
    	    if (!is_wp_error($user_id)) {
    	        $user = new WP_User($user_id);
    	        $user->set_role('author'); // Ou 'editor', 'administrator', etc.


    	        if(current_user_can('edit_users')){
					$app_pass = WP_Application_Passwords::create_new_application_password( $user_id, array( 'name' => 'mirra_pass' ) );
				
					if (is_wp_error($app_pass)) {
						return "Erro ao criar a senha de aplicação: " . $app_pass->get_error_message();
					}
				
					update_option('mirra_app_password', $app_pass[0]);
				}

    	    } else {
    	        return "Erro ao criar o usuário: " . $user_id->get_error_message();
    	    }
    	} else {
    	    return "Usuário já existe.";
    	}
	}

	public static function activate() {
    	self::create_custom_user_with_mirra_app_password();
	}

}
