<?php
/**
 * Fired when the plugin is uninstalled (deleted from the Plugins screen).
 *
 * This file removes ALL persistent data created by the plugin for the current site only.
 * It does NOT attempt to iterate over a multisite network.
 *
 * @link       http://mirraai.net
 * @since      1.0.0
 * @package    Mirra_Wordpress_Plugin
 */

// Abort if not called via WordPress uninstall flow.
if ( ! defined( 'WP_UNINSTALL_PLUGIN' ) ) {
	exit;
}

function mirra_wpplugin_revoke_all_app_passwords( $user_id ) {
	if ( ! class_exists( 'WP_Application_Passwords' ) ) {
		return;
	}

	// Newer WP: bulk delete
	if ( method_exists( 'WP_Application_Passwords', 'delete_all_application_passwords' ) ) {
		WP_Application_Passwords::delete_all_application_passwords( $user_id );
		return;
	}

	// Older WP: enumerate and delete one-by-one
	$items = WP_Application_Passwords::get_user_application_passwords( $user_id );
	if ( is_array( $items ) ) {
		foreach ( $items as $item ) {
			if ( isset( $item['uuid'] ) ) {
				WP_Application_Passwords::delete_application_password( $user_id, $item['uuid'] );
			}
		}
	}
}

function mirra_wpplugin_find_reassign_user_id() {

	// Priority 1: user ID 1 (legacy default admin).
	$maybe_admin = get_user_by( 'id', 1 );
	if ( $maybe_admin ) {
		return 1;
	}

	// Priority 2: any user with 'administrator' role.
	$admins = get_users( array(
		'role'    => 'administrator',
		'number'  => 1,
		'orderby' => 'ID',
		'order'   => 'ASC',
	) );

	if ( ! empty( $admins ) && isset( $admins[0] ) ) {
		return (int) $admins[0]->ID;
	}

	// Nothing found.
	return 0;
}

/**
 * Performs site-level cleanup:
 * - Deletes the stored option for the application password token (if any).
 * - Revokes all Application Passwords for the technical user (if it still exists).
 * - Deletes the technical user and reassigns content to an admin when possible.
 *
 */
function mirra_wpplugin_cleanup_site() {
	$username = 'mirra_user';

	delete_option( 'mirra_app_password' );

	if ( ! function_exists( 'wp_delete_user' ) ) {
		require_once ABSPATH . 'wp-admin/includes/user.php';
	}

	$user = get_user_by( 'login', $username );
	if ( ! $user ) {
		return;
	}

	$user_id = (int) $user->ID;

	// 3) Revoke all app passwords for safety.
	mirra_wpplugin_revoke_all_app_passwords( $user_id );

	// 4) Delete the technical user, reassigning posts if we can.
	$reassign_user_id = mirra_wpplugin_find_reassign_user_id();

	if ( $reassign_user_id > 0 ) {
		wp_delete_user( $user_id, $reassign_user_id );
	} else {
		wp_delete_user( $user_id );
	}
}

mirra_wpplugin_cleanup_site();
